import { useEffect, useState } from "react"
import { Link, useLocation, useNavigate, useParams, useSearchParams } from "react-router-dom"
import { useApi, useSession } from "./auth"
import { Alert, Page, Panel, PasswordField, Status, field, ghost, primary } from "./ui"

function initial(name) {
  return (name || "?").trim().charAt(0).toUpperCase()
}

export function Prosumers() {
  const call = useApi()
  const { session } = useSession()
  const location = useLocation()
  const isBackoffice = session.user.role === "Backoffice"
  const [params, setParams] = useSearchParams()
  const [query, setQuery] = useState(params.get("q") || "")
  const [status, setStatus] = useState(params.get("status") || "")
  const [rows, setRows] = useState([])
  const [error, setError] = useState("")
  const [flash, setFlash] = useState(location.state?.flash || "")

  function load(nextStatus = status, nextQuery = query) {
    const search = new URLSearchParams()
    if (nextStatus) search.set("status", nextStatus)
    if (nextQuery) search.set("q", nextQuery)
    call(`prosumers${search.toString() ? `?${search}` : ""}`).then(setRows).catch((err) => setError(err.message))
  }
  const searchKey = params.toString()
  useEffect(() => { load(params.get("status") || "", params.get("q") || "") }, [searchKey])

  async function changeStatus(nic, action) {
    if (action === "deactivate" && !window.confirm("Deactivate this prosumer? They cannot log in until Backoffice reactivates them.")) return
    try {
      await call(`prosumers/${encodeURIComponent(nic)}/${action}`, { method: "POST" })
      setFlash(action === "reactivate" ? `Reactivated ${nic}.` : `Deactivated ${nic}.`)
      load()
    } catch (err) { setError(err.message) }
  }

  return (
    <Page eyebrow="Accounts" title="Prosumers" lede={isBackoffice ? "Search by NIC, name, username or phone. Reactivate is available only to Backoffice." : "You can look up accounts. Only Backoffice can reactivate one."} action={isBackoffice && <Link className={primary} to="/prosumers/new">Create prosumer</Link>}>
      {flash && <Alert tone="success">{flash}</Alert>}
      {error && <Alert>{error}</Alert>}
      <form className="mb-3 grid gap-2 rounded-2xl border border-line bg-card p-3 md:grid-cols-[1fr_180px_auto]" onSubmit={(event) => { event.preventDefault(); setParams({ q: query, status }) }}>
        <input className={field} value={query} placeholder="NIC, name, username or phone" onChange={(event) => setQuery(event.target.value)} />
        <select className={field} value={status} onChange={(event) => setStatus(event.target.value)}>
          <option value="">All statuses</option>
          <option>Pending</option>
          <option>Active</option>
          <option>Deactivated</option>
        </select>
        <div className="flex gap-2"><button className={primary} type="submit">Search</button><Link className={ghost} to="/prosumers">Clear</Link></div>
      </form>
      <Panel className="overflow-x-auto">
        <table className="w-full min-w-[760px] text-left">
          <thead className="bg-[#faf6ee] text-xs tracking-[0.08em] text-muted uppercase"><tr><th className="px-4 py-3">Prosumer</th><th>NIC</th><th>Username</th><th>Phone</th><th>Status</th>{isBackoffice && <th className="px-4 text-right">Actions</th>}</tr></thead>
          <tbody>
            {rows.length === 0 && <tr><td className="px-4 py-8 text-center text-muted" colSpan={6}>No prosumers match this search.</td></tr>}
            {rows.map((user) => (
              <tr key={user.nic || user.id} className="border-t border-[#efe7d8]">
                <td className="px-4 py-3"><span className="mr-2 inline-grid h-8 w-8 place-items-center rounded-full bg-[#f3e2c2] font-bold text-[#6a4308]">{initial(user.fullName)}</span>{user.fullName}</td>
                <td>{user.nic}</td>
                <td>{user.username}</td>
                <td>{user.phone}</td>
                <td><Status value={user.status} /></td>
                {isBackoffice && (
                  <td className="px-4 text-right whitespace-nowrap">
                    <Link className={`${ghost} mr-2 px-3 py-1 text-sm`} to={`/prosumers/${user.nic}/edit`}>Edit</Link>
                    {user.status === "Deactivated"
                      ? <button className="rounded-xl bg-leaf px-3 py-1 text-sm font-semibold text-white" type="button" onClick={() => changeStatus(user.nic, "reactivate")}>Reactivate</button>
                      : <button className="rounded-xl border border-red-200 px-3 py-1 text-sm text-red-700" type="button" onClick={() => changeStatus(user.nic, "deactivate")}>Deactivate</button>}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </Panel>
    </Page>
  )
}

export function ProsumerForm() {
  const { nic } = useParams()
  const editing = Boolean(nic)
  const call = useApi()
  const navigate = useNavigate()
  const [form, setForm] = useState({ nic: nic || "", username: "", fullName: "", email: "", phone: "", password: "" })
  const [error, setError] = useState("")
  useEffect(() => {
    if (!editing) return
    call(`prosumers/${encodeURIComponent(nic)}`).then((user) => setForm({ nic: user.nic, username: user.username, fullName: user.fullName, email: user.email || "", phone: user.phone || "", password: "" })).catch((err) => setError(err.message))
  }, [nic])
  function set(key, value) { setForm((current) => ({ ...current, [key]: value })) }

  async function submit(event) {
    event.preventDefault()
    if (!editing && form.password.length < 8) { setError("Password is required and must be at least 8 characters."); return }
    const body = { username: form.username, fullName: form.fullName, email: form.email, phone: form.phone }
    if (form.password) body.password = form.password
    try {
      if (editing) await call(`prosumers/${encodeURIComponent(nic)}`, { method: "PUT", body })
      else await call("prosumers", { method: "POST", body: { ...body, nic: form.nic, password: form.password } })
      navigate("/prosumers", { state: { flash: editing ? `Updated ${nic}.` : `Created active prosumer ${form.nic}.` } })
    } catch (err) { setError(err.message) }
  }

  return (
    <Page eyebrow="Prosumers" title={editing ? `Edit ${nic}` : "Create prosumer"} lede={editing ? "NIC and role stay as they are. Email and phone are checked by the API." : "Staff-created accounts start as Active. Self-registration on the phone starts as Pending."} action={<Link className={ghost} to="/prosumers">Back to list</Link>}>
      <form className="rounded-2xl border border-line bg-card p-5" onSubmit={submit}>
        {error && <Alert>{error}</Alert>}
        <div className="grid gap-3 md:grid-cols-2">
          <label><span className="mb-1 block text-sm font-semibold">NIC</span><input className={field} value={form.nic} required readOnly={editing} placeholder="200012345678" onChange={(event) => set("nic", event.target.value)} /><span className="mt-1 block text-sm text-muted">{editing ? "NIC is the primary key and cannot change." : "Old NIC (9 digits plus V or X) or new 12-digit NIC."}</span></label>
          <label><span className="mb-1 block text-sm font-semibold">Username</span><input className={field} required value={form.username} onChange={(event) => set("username", event.target.value)} /></label>
          <label className="md:col-span-2"><span className="mb-1 block text-sm font-semibold">Full name</span><input className={field} required value={form.fullName} onChange={(event) => set("fullName", event.target.value)} /></label>
          <label><span className="mb-1 block text-sm font-semibold">Email</span><input className={field} type="email" value={form.email} onChange={(event) => set("email", event.target.value)} /></label>
          <label><span className="mb-1 block text-sm font-semibold">Phone</span><input className={field} value={form.phone} onChange={(event) => set("phone", event.target.value)} /></label>
          <div className="md:col-span-2"><PasswordField label={editing ? "New password" : "Password"} value={form.password} autoComplete="new-password" hint={editing ? "Leave blank to keep the current password." : "At least 8 characters. Created accounts are Active immediately."} onChange={(value) => set("password", value)} /></div>
        </div>
        <div className="mt-5 flex gap-2"><button className={primary} type="submit">{editing ? "Save changes" : "Create active account"}</button><Link className={ghost} to="/prosumers">Cancel</Link></div>
      </form>
    </Page>
  )
}

export function Pending() {
  const call = useApi()
  const [rows, setRows] = useState([])
  const [error, setError] = useState("")
  const [flash, setFlash] = useState("")
  function load() { call("prosumers/pending").then(setRows).catch((err) => setError(err.message)) }
  useEffect(() => { load() }, [])

  async function decide(nic, action) {
    if (action === "reject" && !window.confirm("Reject this registration? The account will be deactivated.")) return
    const path = action === "activate" ? `prosumers/${encodeURIComponent(nic)}/activate` : `prosumers/${encodeURIComponent(nic)}/deactivate`
    try {
      await call(path, { method: "POST" })
      setFlash(action === "activate" ? `Activated ${nic}.` : `Rejected ${nic}.`)
      load()
    } catch (err) { setError(err.message) }
  }

  return (
    <Page eyebrow="Approvals" title="Pending activations" lede="These prosumers registered on the phone. Activate them so they can log in, or reject the registration." action={<span className="rounded-full bg-[#f8edd4] px-3 py-1 text-sm font-bold text-[#8a5a08]">{rows.length} waiting</span>}>
      {flash && <Alert tone="success">{flash}</Alert>}
      {error && <Alert>{error}</Alert>}
      <Panel className="overflow-x-auto">
        <table className="w-full min-w-[720px] text-left">
          <thead className="bg-[#faf6ee] text-xs tracking-[0.08em] text-muted uppercase"><tr><th className="px-4 py-3">Prosumer</th><th>NIC</th><th>Username</th><th>Contact</th><th className="px-4 text-right">Decision</th></tr></thead>
          <tbody>
            {rows.length === 0 && <tr><td className="px-4 py-8 text-center text-muted" colSpan={5}>No accounts are waiting for activation.</td></tr>}
            {rows.map((user) => (
              <tr key={user.nic} className="border-t border-[#efe7d8]">
                <td className="px-4 py-3"><span className="mr-2 inline-grid h-8 w-8 place-items-center rounded-full bg-[#f3e2c2] font-bold text-[#6a4308]">{initial(user.fullName)}</span>{user.fullName}</td>
                <td>{user.nic}</td>
                <td>{user.username}</td>
                <td>{user.email}<div className="text-sm text-muted">{user.phone}</div></td>
                <td className="px-4 text-right whitespace-nowrap">
                  <button className="mr-2 rounded-xl bg-leaf px-3 py-1 text-sm font-semibold text-white" type="button" onClick={() => decide(user.nic, "activate")}>Activate</button>
                  <button className="rounded-xl border border-red-200 px-3 py-1 text-sm text-red-700" type="button" onClick={() => decide(user.nic, "reject")}>Reject</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Panel>
    </Page>
  )
}
