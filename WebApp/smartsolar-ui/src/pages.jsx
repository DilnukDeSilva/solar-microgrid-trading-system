import { useEffect, useState } from "react"
import { Link, useLocation, useNavigate, useParams } from "react-router-dom"
import { ApiError, api } from "./api"
import { homeFor, useApi, useSession } from "./auth"
import { Alert, BrandPanel, Page, Panel, PasswordField, Status, field, ghost, primary } from "./ui"

function initial(name) {
  return (name || "?").trim().charAt(0).toUpperCase()
}

export function Welcome() {
  return (
    <div className="grid min-h-screen md:grid-cols-2">
      <BrandPanel />
      <section className="flex items-center bg-paper px-6 py-10">
        <div className="mx-auto w-full max-w-md">
          <p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Opening screen</p>
          <h2 className="mt-1 font-display text-4xl">Sign in to the desk</h2>
          <p className="mt-2 text-muted">Use a Backoffice or Grid Operator account. Prosumer logins belong on the phone.</p>
          <Link className={`${primary} mt-6`} to="/login">Staff log in</Link>
          <p className="mt-4 text-sm text-muted">Seed accounts: <code>admin</code> · <code>operator1</code></p>
        </div>
      </section>
    </div>
  )
}

export function Login() {
  const { session, signIn } = useSession()
  const navigate = useNavigate()
  const location = useLocation()
  const [username, setUsername] = useState("")
  const [password, setPassword] = useState("")
  const [error, setError] = useState(location.state?.flash || "")

  useEffect(() => {
    if (session) navigate(homeFor(session.user.role), { replace: true })
  }, [session, navigate])

  async function submit(event) {
    event.preventDefault()
    setError("")
    try {
      const result = await api("auth/login", { method: "POST", body: { username: username.trim(), password } })
      signIn(result)
      navigate(homeFor(result.user.role), { replace: true })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Login failed.")
    }
  }

  return (
    <div className="grid min-h-screen md:grid-cols-2">
      <BrandPanel />
      <section className="flex items-center bg-paper px-6 py-10">
        <form className="mx-auto w-full max-w-md" onSubmit={submit}>
          <p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Staff access</p>
          <h2 className="mt-1 font-display text-4xl">Log in</h2>
          <p className="mt-2 mb-5 text-muted">Username, or a prosumer NIC if you are checking that mobile accounts stay off this desk.</p>
          {error && <Alert>{error}</Alert>}
          <label className="mb-3 block"><span className="mb-1 block text-sm font-semibold">Username or NIC</span><input className={field} value={username} autoFocus autoComplete="username" placeholder="admin" onChange={(event) => setUsername(event.target.value)} /></label>
          <div className="mb-4"><PasswordField id="password" label="Password" value={password} autoComplete="current-password" onChange={setPassword} /></div>
          <button className={`${primary} w-full`} type="submit">Enter the desk</button>
          <p className="mt-4 text-sm"><Link className="text-sun" to="/">Back to the opening screen</Link></p>
        </form>
      </section>
    </div>
  )
}

export function Denied() {
  const { signOut } = useSession()
  const navigate = useNavigate()
  return (
    <div className="mx-auto max-w-xl px-6 py-16">
      <p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Access</p>
      <h1 className="font-display text-4xl">This desk is not for that role</h1>
      <p className="mt-2 text-muted">Backoffice and Grid Operator see different menus. Prosumer accounts sign in on the Android app.</p>
      <button className={`${primary} mt-6`} type="button" onClick={() => { signOut(); navigate("/login") }}>Log out</button>
    </div>
  )
}

export function Desk() {
  const call = useApi()
  const [counts, setCounts] = useState({ staff: 0, prosumers: 0, active: 0, pending: 0 })
  const [error, setError] = useState("")
  useEffect(() => {
    Promise.all([call("users"), call("prosumers"), call("prosumers/pending")])
      .then(([staff, prosumers, pending]) => setCounts({
        staff: staff.length,
        prosumers: prosumers.length,
        active: prosumers.filter((user) => user.status === "Active").length,
        pending: pending.length,
      }))
      .catch((err) => setError(err.message))
  }, [])

  const cards = [
    ["/staff", counts.staff, "Staff accounts"],
    ["/prosumers", counts.prosumers, "Prosumers"],
    ["/prosumers?status=Active", counts.active, "Active prosumers"],
    ["/pending", counts.pending, "Awaiting activation"],
  ]
  return (
    <Page eyebrow="Backoffice" title="Account desk" lede="Staff accounts, solar prosumers, and registrations waiting to be switched on.">
      {error && <Alert tone="warning">Counts could not be loaded. {error}</Alert>}
      <div className="mb-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {cards.map(([to, value, label]) => (
          <Link key={label} to={to} className="rounded-2xl border border-line bg-card p-4 hover:border-[#d7b56a]">
            <div className="font-display text-4xl">{value}</div>
            <div className="text-sm text-muted">{label}</div>
          </Link>
        ))}
      </div>
      <div className="grid gap-3 md:grid-cols-2">
        <Link className="rounded-2xl border border-line bg-card p-5 hover:border-[#d7b56a]" to="/staff/new"><p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Staff</p><h2 className="font-display text-2xl">New staff account</h2><p className="text-muted">Create a Backoffice officer or a Grid Operator.</p></Link>
        <Link className="rounded-2xl border border-line bg-card p-5 hover:border-[#d7b56a]" to="/prosumers/new"><p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Prosumers</p><h2 className="font-display text-2xl">Register a prosumer</h2><p className="text-muted">NIC is the key. An account created here is Active immediately.</p></Link>
        <Link className="rounded-2xl border border-line bg-card p-5 hover:border-[#d7b56a]" to="/pending"><p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Approvals</p><h2 className="font-display text-2xl">Pending activations</h2><p className="text-muted">Self-registered mobile accounts stay Pending until you activate or reject them.</p></Link>
        <Link className="rounded-2xl border border-line bg-card p-5 hover:border-[#d7b56a]" to="/book"><p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Trading</p><h2 className="font-display text-2xl">Book a slot</h2><p className="text-muted">Reserve a charging window for a prosumer who calls or visits.</p></Link>
      </div>
    </Page>
  )
}

export function Operations() {
  return (
    <Page eyebrow="Grid operator" title="Operations desk" lede="Look up a prosumer and book an energy slot on their behalf.">
      <div className="grid gap-3 md:grid-cols-2">
        <Link className="rounded-2xl border border-line bg-card p-5 hover:border-[#d7b56a]" to="/prosumers"><p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Accounts</p><h2 className="font-display text-2xl">Find a prosumer</h2><p className="text-muted">Search by NIC, name, username or phone. Reactivation stays with Backoffice.</p></Link>
        <Link className="rounded-2xl border border-line bg-card p-5 hover:border-[#d7b56a]" to="/book"><p className="text-xs font-bold tracking-[0.14em] text-sun uppercase">Trading</p><h2 className="font-display text-2xl">Book a slot</h2><p className="text-muted">The API enforces the booking rules and returns the message if a booking is not allowed.</p></Link>
      </div>
    </Page>
  )
}

export function StaffList() {
  const call = useApi()
  const location = useLocation()
  const [users, setUsers] = useState([])
  const [error, setError] = useState("")
  const [flash, setFlash] = useState(location.state?.flash || "")
  useEffect(() => { call("users").then(setUsers).catch((err) => setError(err.message)) }, [])
  return (
    <Page eyebrow="Backoffice" title="Staff users" lede="Backoffice officers and Grid Operators. You cannot deactivate your own account." action={<Link className={primary} to="/staff/new">New staff account</Link>}>
      {flash && <Alert tone="success">{flash}</Alert>}
      {error && <Alert>{error}</Alert>}
      <Panel className="overflow-x-auto">
        <table className="w-full min-w-[720px] text-left">
          <thead className="bg-[#faf6ee] text-xs tracking-[0.08em] text-muted uppercase"><tr><th className="px-4 py-3">Name</th><th>Username</th><th>Role</th><th>Status</th><th>Contact</th><th className="px-4 text-right">Actions</th></tr></thead>
          <tbody>
            {users.length === 0 && <tr><td className="px-4 py-8 text-center text-muted" colSpan={6}>No staff accounts came back from the API.</td></tr>}
            {users.map((user) => (
              <tr key={user.id} className="border-t border-[#efe7d8]">
                <td className="px-4 py-3"><span className="mr-2 inline-grid h-8 w-8 place-items-center rounded-full bg-[#f3e2c2] font-bold text-[#6a4308]">{initial(user.fullName)}</span>{user.fullName}</td>
                <td>{user.username}</td>
                <td><Status value={user.role} /></td>
                <td><Status value={user.status} /></td>
                <td>{user.email}<div className="text-sm text-muted">{user.phone}</div></td>
                <td className="px-4 text-right whitespace-nowrap">
                  <Link className={`${ghost} mr-2 px-3 py-1 text-sm`} to={`/staff/${user.id}/edit`}>Edit</Link>
                  {user.status !== "Deactivated" && <Link className="rounded-xl border border-red-200 px-3 py-1 text-sm text-red-700" to={`/staff/${user.id}/deactivate`}>Deactivate</Link>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Panel>
    </Page>
  )
}

export function StaffForm() {
  const { id } = useParams()
  const editing = Boolean(id)
  const call = useApi()
  const navigate = useNavigate()
  const [form, setForm] = useState({ username: "", password: "", fullName: "", email: "", phone: "", role: "GridOperator" })
  const [error, setError] = useState("")
  useEffect(() => {
    if (!editing) return
    call(`users/${encodeURIComponent(id)}`).then((user) => setForm({ username: user.username, password: "", fullName: user.fullName, email: user.email || "", phone: user.phone || "", role: user.role })).catch((err) => setError(err.message))
  }, [id])

  function set(key, value) { setForm((current) => ({ ...current, [key]: value })) }

  async function submit(event) {
    event.preventDefault()
    setError("")
    if (!editing && form.password.length < 8) { setError("Password is required and must be at least 8 characters."); return }
    const body = { fullName: form.fullName, email: form.email, phone: form.phone, role: form.role }
    if (form.password) body.password = form.password
    if (!editing) body.username = form.username
    try {
      if (editing) await call(`users/${encodeURIComponent(id)}`, { method: "PUT", body })
      else await call("users", { method: "POST", body: { ...body, username: form.username, password: form.password } })
      navigate("/staff", { state: { flash: editing ? `Updated ${form.username}.` : `Created staff account ${form.username}.` } })
    } catch (err) { setError(err.message) }
  }

  return (
    <Page eyebrow="Staff" title={editing ? `Edit ${form.username || "account"}` : "New staff account"} lede="Roles are Backoffice or Grid Operator. Prosumers are created on the prosumer screen, keyed by NIC.">
      <form className="rounded-2xl border border-line bg-card p-5" onSubmit={submit}>
        {error && <Alert>{error}</Alert>}
        <div className="grid gap-3 md:grid-cols-2">
          <label className="block"><span className="mb-1 block text-sm font-semibold">Username</span>{editing ? <p className="py-2">{form.username}</p> : <input className={field} value={form.username} onChange={(event) => set("username", event.target.value)} required />}</label>
          <label className="block"><span className="mb-1 block text-sm font-semibold">Role</span>
            <select className={field} value={form.role} onChange={(event) => set("role", event.target.value)}>
              <option value="Backoffice">Backoffice</option>
              <option value="GridOperator">Grid operator</option>
            </select>
          </label>
          <label className="block md:col-span-2"><span className="mb-1 block text-sm font-semibold">Full name</span><input className={field} value={form.fullName} required onChange={(event) => set("fullName", event.target.value)} /></label>
          <label className="block"><span className="mb-1 block text-sm font-semibold">Email</span><input className={field} type="email" value={form.email} onChange={(event) => set("email", event.target.value)} /></label>
          <label className="block"><span className="mb-1 block text-sm font-semibold">Phone</span><input className={field} value={form.phone} onChange={(event) => set("phone", event.target.value)} /></label>
          <div className="md:col-span-2"><PasswordField label={editing ? "New password" : "Password"} value={form.password} autoComplete="new-password" hint={editing ? "Leave blank to keep the current password." : "At least 8 characters. The API stores only the hash."} onChange={(value) => set("password", value)} /></div>
        </div>
        <div className="mt-5 flex gap-2"><button className={primary} type="submit">Save account</button><Link className={ghost} to="/staff">Cancel</Link></div>
      </form>
    </Page>
  )
}

export function StaffDeactivate() {
  const { id } = useParams()
  const call = useApi()
  const navigate = useNavigate()
  const [user, setUser] = useState(null)
  const [error, setError] = useState("")
  useEffect(() => { call(`users/${encodeURIComponent(id)}`).then(setUser).catch((err) => setError(err.message)) }, [id])
  async function deactivate() {
    try {
      const result = await call(`users/${encodeURIComponent(id)}/deactivate`, { method: "POST" })
      navigate("/staff", { state: { flash: `${result.username} is now Deactivated.` } })
    } catch (err) { setError(err.message) }
  }
  if (!user) return <Page title="Deactivate staff account">{error && <Alert>{error}</Alert>}</Page>
  return (
    <div className="max-w-xl">
      <Page eyebrow="Staff" title={`Deactivate ${user.username}?`} lede="The API sets the status to Deactivated. That account can no longer log in. You cannot deactivate the account you are using.">
        {error && <Alert>{error}</Alert>}
        <Panel className="mb-4 p-5"><p>{user.fullName}</p><p className="mt-2"><Status value={user.role} /> <Status value={user.status} /></p></Panel>
        <button className="rounded-xl bg-red-700 px-4 py-2.5 font-semibold text-white" type="button" onClick={deactivate}>Deactivate account</button>
        <Link className={`${ghost} ml-2`} to="/staff">Cancel</Link>
      </Page>
    </div>
  )
}
