import { Link, Outlet } from 'react-router-dom'

export default function App() {
  return (
      <div style={{ maxWidth: 900, margin: '0 auto', padding: '1rem', fontFamily: 'system-ui, sans-serif' }}>
        <header style={{ display: 'flex', gap: '1rem', alignItems: 'baseline', marginBottom: '1.5rem' }}>
          <Link to="/" style={{ fontWeight: 700, fontSize: '1.25rem', textDecoration: 'none' }}>🏁 F1 Stats</Link>
          <Link to="/seasons/2026">2026 races</Link>
        </header>
        <Outlet />
      </div>
  )
}