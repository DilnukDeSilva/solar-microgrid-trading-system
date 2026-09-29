import { useEffect, useState } from "react"
import { useLocation } from "react-router-dom"
import { useApi } from "./auth"
import { Alert, Page, Panel, field, primary } from "./ui"

function localTime(iso) {
  return new Intl.DateTimeFormat("en-GB", { timeZone: "Asia/Colombo", hour: "2-digit", minute: "2-digit", hourCycle: "h23" }).format(new Date(iso))
}

function localDay(iso) {
  return new Intl.DateTimeFormat("en-GB", { timeZone: "Asia/Colombo", weekday: "long", day: "numeric", month: "long" }).format(new Date(iso))
}

export function BookSlot() {
  const call = useApi()
  const location = useLocation()
  const [stations, setStations] = useState([])
  const [nic, setNic] = useState("")
  const [stationId, setStationId] = useState("")
  const [date, setDate] = useState("")
  const [slots, setSlots] = useState(null)
  const [slotId, setSlotId] = useState("")
  const [error, setError] = useState("")
  const [booking, setBooking] = useState(location.state?.booking || null)

  useEffect(() => { call("reservations/bookable-stations").then(setStations).catch((err) => setError(err.message)) }, [])

  async function showSlots(event) {
    event.preventDefault()
    setError("")
    setBooking(null)
    const query = new URLSearchParams({ stationId })
    if (date) query.set("date", date)
    try {
      const rows = await call(`reservations/available-slots?${query}`)
      setSlots(rows)
      setSlotId("")
    } catch (err) { setError(err.message) }
  }

  async function confirm(event) {
    event.preventDefault()
    setError("")
    try {
      const created = await call("reservations", { method: "POST", body: { prosumerNic: nic.trim(), stationId, slotId } })
      setBooking(created)
      setSlots(null)
    } catch (err) { setError(err.message) }
  }

  const groups = slots ? Object.groupBy(slots, (slot) => localDay(slot.startTime)) : {}

  return (
    <Page eyebrow="Trading" title="Book a slot for a prosumer" lede="For prosumers who call or visit. The API checks the booking rules and tells you if something is not allowed.">
      {error && <Alert>{error}</Alert>}
      {booking && (
        <Panel className="mb-4 p-5">
          <h2 className="font-display text-2xl">Booking created</h2>
          <dl className="mt-3 grid gap-2 sm:grid-cols-2">
            <div><dt className="text-sm text-muted">Reference</dt><dd>{booking.id}</dd></div>
            <div><dt className="text-sm text-muted">Status</dt><dd>{booking.status}</dd></div>
            <div><dt className="text-sm text-muted">Prosumer NIC</dt><dd>{booking.prosumerNic}</dd></div>
            <div><dt className="text-sm text-muted">Station</dt><dd>{booking.stationName}</dd></div>
            <div><dt className="text-sm text-muted">Slot</dt><dd>{localDay(booking.scheduledAt)} · {localTime(booking.scheduledAt)}</dd></div>
          </dl>
        </Panel>
      )}
      <div className="grid gap-4 lg:grid-cols-2">
        <form className="rounded-2xl border border-line bg-card p-5" onSubmit={showSlots}>
          <h2 className="mb-3 font-semibold">1. Prosumer and station</h2>
          <label className="mb-3 block"><span className="mb-1 block text-sm font-semibold">Prosumer NIC</span><input className={field} required value={nic} placeholder="200012345678" onChange={(event) => setNic(event.target.value)} /></label>
          <label className="mb-3 block"><span className="mb-1 block text-sm font-semibold">Station</span>
            <select className={field} required value={stationId} onChange={(event) => setStationId(event.target.value)}>
              <option value="">Choose a station</option>
              {stations.map((station) => <option key={station.id} value={station.id}>{station.name}</option>)}
            </select>
          </label>
          <label className="mb-3 block"><span className="mb-1 block text-sm font-semibold">Date</span><input className={field} type="date" value={date} onChange={(event) => setDate(event.target.value)} /><span className="mt-1 block text-sm text-muted">Optional. Leave empty to see every free slot you can book.</span></label>
          <button className={`${primary} bg-white text-sun ring-1 ring-sun hover:bg-[#fff6e8]`} type="submit">Show free slots</button>
        </form>
        <form className="rounded-2xl border border-line bg-card p-5" onSubmit={confirm}>
          <h2 className="mb-3 font-semibold">2. Pick a slot and confirm</h2>
          {slots === null && <p className="text-muted">Enter the prosumer&apos;s NIC and choose a station to see its free slots.</p>}
          {slots?.length === 0 && <p className="text-muted">No free slots found. Try another date or station.</p>}
          {slots?.length > 0 && (
            <>
              {Object.entries(groups).map(([day, daySlots]) => (
                <div key={day} className="mb-3">
                  <div className="mb-2 text-sm font-semibold text-muted">{day}</div>
                  <div className="flex flex-wrap gap-2">
                    {daySlots.map((slot) => (
                      <label key={slot.id} className={`cursor-pointer rounded-xl border px-3 py-2 text-sm ${slotId === slot.id ? "border-leaf bg-[#e5f3eb]" : "border-line"}`}>
                        <input className="sr-only" type="radio" name="slot" value={slot.id} required checked={slotId === slot.id} onChange={() => setSlotId(slot.id)} />
                        {localTime(slot.startTime)} – {localTime(slot.endTime)}
                      </label>
                    ))}
                  </div>
                </div>
              ))}
              <button className={primary} type="submit">Confirm booking</button>
            </>
          )}
        </form>
      </div>
    </Page>
  )
}
