import { useState } from "react"

export const field = "w-full rounded-xl border border-line bg-white px-3 py-2.5 text-ink outline-none focus:border-gold focus:ring-4 focus:ring-gold/20"
export const primary = "inline-flex items-center justify-center rounded-xl bg-sun px-4 py-2.5 font-semibold text-[#fffaf0] hover:bg-[#8d5808]"
export const ghost = "inline-flex items-center justify-center rounded-xl border border-line bg-white px-4 py-2.5 font-semibold text-ink hover:border-[#d7b56a]"

export function Status({ value }) {
  const kind = String(value || "").toLowerCase()
  const tone = {
    active: "bg-[#e5f3eb] text-leaf",
    pending: "bg-[#f8edd4] text-[#8a5a08]",
    backoffice: "bg-[#e3f2ee] text-[#1d4e46]",
    gridoperator: "bg-[#f6ead4] text-[#5d3d10]",
  }[kind] || "bg-[#ece7de] text-[#5e574e]"
  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-bold tracking-wide uppercase before:h-1.5 before:w-1.5 before:rounded-full before:bg-current ${tone}`}>
      {value || "Unknown"}
    </span>
  )
}

export function Page({ eyebrow, title, lede, action, children }) {
  return (
    <section>
      <header className="mb-5 flex flex-col items-start gap-3 sm:flex-row sm:items-end sm:justify-between">
        <div>
          {eyebrow && <p className="mb-1 text-xs font-bold tracking-[0.14em] text-sun uppercase">{eyebrow}</p>}
          <h1 className="font-display text-3xl">{title}</h1>
          {lede && <p className="mt-1 max-w-2xl text-muted">{lede}</p>}
        </div>
        {action}
      </header>
      {children}
    </section>
  )
}

export function Alert({ children, tone = "danger" }) {
  const toneClass = tone === "success" ? "border-leaf/30 bg-[#e5f3eb] text-leaf" : tone === "warning" ? "border-[#e2c98a] bg-[#fbf3df] text-[#6d4a08]" : "border-red-200 bg-red-50 text-red-800"
  return <div className={`mb-4 rounded-xl border px-4 py-3 ${toneClass}`}>{children}</div>
}

export function Panel({ children, className = "" }) {
  return <div className={`rounded-2xl border border-line bg-card shadow-[0_12px_30px_rgba(28,25,20,0.04)] ${className}`}>{children}</div>
}

export function BrandPanel() {
  return (
    <section className="relative flex flex-col justify-between overflow-hidden bg-[radial-gradient(circle_at_18%_18%,rgba(227,155,22,0.45),transparent_42%),linear-gradient(165deg,#1b3a2b,#102119_55%,#0c1812)] p-7 text-[#f7f1e4] md:p-10">
      <div>
        <p className="text-xs font-bold tracking-[0.16em] text-[#e8c887] uppercase">Smart Solar · SE4040</p>
        <h1 className="mt-3 max-w-[10ch] font-display text-5xl leading-none text-[#fff8ea] md:text-6xl">The microgrid trading desk.</h1>
        <p className="mt-4 max-w-md text-[#e6dccb]">Backoffice officers run accounts. Grid operators watch bookings. Prosumers stay on the Android app.</p>
        <ul className="mt-6 hidden border-t border-white/10 md:block">
          <li className="border-b border-white/10 py-3"><strong className="block text-[#fff8ea]">Backoffice</strong><span className="text-[#cfc3ae]">Staff users, prosumer records, and pending activations.</span></li>
          <li className="py-3"><strong className="block text-[#fff8ea]">Grid operator</strong><span className="text-[#cfc3ae]">Look up prosumers and book a charging slot on their behalf.</span></li>
        </ul>
      </div>
      <p className="relative z-10 mt-8 text-sm text-[#cfc3ae]">This desk talks only to the API. It never opens the database.</p>
    </section>
  )
}

export function PasswordField({ id, label, value, onChange, hint, autoComplete }) {
  const [shown, setShown] = useState(false)
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-semibold">{label}</span>
      <span className="flex">
        <input id={id} className={`${field} rounded-r-none`} type={shown ? "text" : "password"} value={value} autoComplete={autoComplete} onChange={(event) => onChange(event.target.value)} />
        <button type="button" className="rounded-r-xl border border-l-0 border-line bg-white px-3 text-sm font-semibold" onClick={() => setShown((next) => !next)}>{shown ? "Hide" : "Show"}</button>
      </span>
      {hint && <span className="mt-1 block text-sm text-muted">{hint}</span>}
    </label>
  )
}
