import { Link, Outlet } from 'react-router-dom'
import { YearProvider, useYear } from './YearContext'

function TopbarControls() {
    const { year, setYear, seasons } = useYear()
    return (
        <>
            <nav className="nav"><Link to={`/seasons/${year}`}>Races</Link></nav>
            <span className="spacer" />
            <select className="year-select" value={year} onChange={e => setYear(Number(e.target.value))}>
                {seasons.map(s => <option key={s} value={s}>{s}</option>)}
            </select>
        </>
    )
}

export default function App() {
    return (
        <YearProvider>
            <header className="topbar">
                <div className="topbar__inner">
                    <Link to="/" className="wordmark">F1 Stats</Link>
                    <TopbarControls />
                </div>
            </header>
            <main className="container"><Outlet /></main>
        </YearProvider>
    )
}