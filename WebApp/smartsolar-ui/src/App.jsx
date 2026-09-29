import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"
import { ProsumerForm, Prosumers, Pending } from "./accounts"
import { RequireAuth, SessionProvider } from "./auth"
import { BookSlot } from "./book"
import { Denied, Desk, Login, Operations, StaffDeactivate, StaffForm, StaffList, Welcome } from "./pages"
import { Shell } from "./Shell"

function DeskShell({ children }) {
  return <RequireAuth roles={["Backoffice", "GridOperator"]}><Shell>{children}</Shell></RequireAuth>
}

export default function App() {
  return (
    <SessionProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Welcome />} />
          <Route path="/login" element={<Login />} />
          <Route path="/denied" element={<Denied />} />
          <Route path="/desk" element={<DeskShell><RequireAuth roles={["Backoffice"]}><Desk /></RequireAuth></DeskShell>} />
          <Route path="/operations" element={<DeskShell><RequireAuth roles={["GridOperator"]}><Operations /></RequireAuth></DeskShell>} />
          <Route path="/staff" element={<DeskShell><RequireAuth roles={["Backoffice"]}><StaffList /></RequireAuth></DeskShell>} />
          <Route path="/staff/new" element={<DeskShell><RequireAuth roles={["Backoffice"]}><StaffForm /></RequireAuth></DeskShell>} />
          <Route path="/staff/:id/edit" element={<DeskShell><RequireAuth roles={["Backoffice"]}><StaffForm /></RequireAuth></DeskShell>} />
          <Route path="/staff/:id/deactivate" element={<DeskShell><RequireAuth roles={["Backoffice"]}><StaffDeactivate /></RequireAuth></DeskShell>} />
          <Route path="/prosumers" element={<DeskShell><Prosumers /></DeskShell>} />
          <Route path="/prosumers/new" element={<DeskShell><RequireAuth roles={["Backoffice"]}><ProsumerForm /></RequireAuth></DeskShell>} />
          <Route path="/prosumers/:nic/edit" element={<DeskShell><RequireAuth roles={["Backoffice"]}><ProsumerForm /></RequireAuth></DeskShell>} />
          <Route path="/pending" element={<DeskShell><RequireAuth roles={["Backoffice"]}><Pending /></RequireAuth></DeskShell>} />
          <Route path="/book" element={<DeskShell><BookSlot /></DeskShell>} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </SessionProvider>
  )
}
