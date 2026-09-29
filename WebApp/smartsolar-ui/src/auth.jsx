import { createContext, useContext, useMemo, useState } from "react"
import { Navigate, useLocation, useNavigate } from "react-router-dom"
import { api, ApiError } from "./api"

const KEY = "smartsolar.session"
const SessionContext = createContext(null)

function readSession() {
  try {
    const raw = sessionStorage.getItem(KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

export function SessionProvider({ children }) {
  const [session, setSession] = useState(readSession)

  const value = useMemo(() => ({
    session,
    signIn(next) {
      sessionStorage.setItem(KEY, JSON.stringify(next))
      setSession(next)
    },
    signOut() {
      sessionStorage.removeItem(KEY)
      setSession(null)
    },
  }), [session])

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}

export function useSession() {
  return useContext(SessionContext)
}

export function homeFor(role) {
  if (role === "Backoffice") return "/desk"
  if (role === "GridOperator") return "/operations"
  return "/denied"
}

export function useApi() {
  const { session, signOut } = useSession()
  const navigate = useNavigate()

  return async (path, options = {}) => {
    try {
      return await api(path, { ...options, token: session?.token })
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        signOut()
        navigate("/login", { replace: true, state: { flash: "Your session expired. Please sign in again." } })
      }
      throw error
    }
  }
}

export function RequireAuth({ roles, children }) {
  const { session } = useSession()
  const location = useLocation()
  if (!session) return <Navigate to="/login" replace state={{ from: location.pathname }} />
  if (roles && !roles.includes(session.user.role)) return <Navigate to="/denied" replace />
  return children
}
