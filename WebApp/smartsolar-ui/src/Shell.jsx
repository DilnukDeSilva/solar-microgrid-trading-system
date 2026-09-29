import { useEffect, useState } from "react"
import { NavLink, useNavigate } from "react-router-dom"
import { useApi, useSession } from "./auth"

function Item({ to, children, badge, onNavigate }) {
  return (
    <NavLink to={to} onClick={onNavigate} className={({ isActive }) => `mb-0.5 flex items-center justify-between rounded-xl px-3 py-2 font-medium ${isActive ? "bg-paper text-night" : "text-[#efe8d8] hover:bg-night-2"}`}>
      <span>{children}</span>
      {badge > 0 && <span className={`min-w-6 rounded-full px-1.5 text-center text-xs font-bold ${badge ? "bg-gold text-[#2a1c04]" : ""}`}>{badge}</span>}
    </NavLink>
  )
}

export function Shell({ children }) {
  const { session, signOut } = useSession()
  const navigate = useNavigate()
  const call = useApi()
  const [open, setOpen] = useState(false)
  const [pending, setPending] = useState(0)
  const role = session.user.role
  const roleLabel = role === "Backoffice" ? "Backoffice" : "Grid operator"

  useEffect(() => {
    if (role !== "Backoffice") return
    call("prosumers/pending").then((rows) => setPending(rows.length)).catch(() => setPending(0))
  }, [role])

  function logout() {
    signOut()
    navigate("/login")
  }

  const nav = (
    <div className="flex h-full min-h-screen w-[268px] flex-col bg-night p-4 text-[#efe8d8]">
      <NavLink to={role === "Backoffice" ? "/desk" : "/operations"} className="mb-4 flex items-center gap-3 px-1" onClick={() => setOpen(false)}>
        <span className="grid h-10 w-10 place-items-center rounded-xl bg-[#243f30] text-gold">☀</span>
        <span>
          <span className="block font-display text-xl leading-none text-[#fff8ea]">Smart Solar</span>
          <span className="text-[0.68rem] tracking-[0.08em] text-[#c8bba4] uppercase">Microgrid desk</span>
        </span>
      </NavLink>
      <p className="mx-2 mt-1 mb-1 text-[0.68rem] tracking-[0.12em] text-[#9e917d] uppercase">Desk</p>
      {role === "Backoffice" && <Item to="/desk" onNavigate={() => setOpen(false)}>Dashboard</Item>}
      {role === "GridOperator" && <Item to="/operations" onNavigate={() => setOpen(false)}>Operations</Item>}
      <p className="mx-2 mt-3 mb-1 text-[0.68rem] tracking-[0.12em] text-[#9e917d] uppercase">Accounts</p>
      {role === "Backoffice" && <Item to="/staff" onNavigate={() => setOpen(false)}>Staff users</Item>}
      <Item to="/prosumers" onNavigate={() => setOpen(false)}>Prosumers</Item>
      {role === "Backoffice" && <Item to="/pending" badge={pending} onNavigate={() => setOpen(false)}>Pending activations</Item>}
      <p className="mx-2 mt-3 mb-1 text-[0.68rem] tracking-[0.12em] text-[#9e917d] uppercase">Trading</p>
      <Item to="/book" onNavigate={() => setOpen(false)}>Book a slot</Item>
      <div className="mt-auto border-t border-white/10 px-2 pt-4">
        <div className="font-semibold text-[#fff8ea]">{session.user.username}</div>
        <div className="mb-3 text-xs tracking-wide text-[#c8bba4] uppercase">{roleLabel}</div>
        <button type="button" className="w-full rounded-xl border border-white/30 px-3 py-2 text-sm hover:bg-[#fff8ea] hover:text-night" onClick={logout}>Log out</button>
      </div>
    </div>
  )

  return (
    <div className="min-h-screen bg-[radial-gradient(900px_280px_at_100%_-10%,rgba(227,155,22,0.16),transparent_60%),var(--color-paper)] lg:flex">
      <aside className="hidden lg:block">{nav}</aside>
      {open && (
        <div className="fixed inset-0 z-40 lg:hidden">
          <button className="absolute inset-0 bg-black/40" aria-label="Close menu" onClick={() => setOpen(false)} />
          <div className="relative z-10 h-full w-[280px]">{nav}</div>
        </div>
      )}
      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center gap-3 bg-night px-4 py-3 text-[#fff8ea] lg:hidden">
          <button type="button" className="rounded-lg border border-white/30 px-3 py-1 text-sm" onClick={() => setOpen(true)}>Menu</button>
          <span className="font-display text-xl">Smart Solar</span>
        </header>
        <main className="mx-auto w-[min(1120px,calc(100%-2rem))] flex-1 py-7">{children}</main>
        <footer className="mx-auto mb-4 w-[min(1120px,calc(100%-2rem))] text-sm text-muted">Smart Solar Microgrid Trading System · this desk talks to the API only</footer>
      </div>
    </div>
  )
}
