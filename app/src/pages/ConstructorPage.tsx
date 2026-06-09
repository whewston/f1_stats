import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, type ConstructorProfile } from '../api'
import { Stat } from '../components/Stat'
import { teamColor } from '../teamColors'
import { Flag } from "../components/Flag"

export default function ConstructorPage() {
    const { constructorId } = useParams()
    const [team, setTeam] = useState<ConstructorProfile | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        setLoading(true)
        api.constructor(constructorId!).then(setTeam).finally(() => setLoading(false))
    }, [constructorId])

    if (loading) return <p className="empty">Loading…</p>
    if (!team) return <p className="empty">Constructor not found.</p>

    const t = team.allTime
    return (
        <section>
            <div className="crumb">Constructor</div>
            <h1 className="page-title"><Flag nationality={team.nationality} /><span className="dot" style={{ background: teamColor(team.constructorId), width: 14, height: 14 }} />{team.name}</h1>            <p className="muted">{team.nationality}</p>

            <div className="stat-grid" style={{ margin: '1.25rem 0 1.75rem' }}>
                <Stat label="Titles" value={t.championships} />
                <Stat label="Wins" value={t.wins} />
                <Stat label="Podiums" value={t.podiums} />
                <Stat label="Poles" value={t.poles} />
                <Stat label="Points" value={t.points} />
                <Stat label="Races" value={t.races} />
                <Stat label="Best" value={t.bestFinish ?? '—'} />
            </div>

            <div className="card">
                <div className="card__head"><span className="card__title">By season</span></div>
                <div className="card__body" style={{ padding: '.3rem .6rem' }}>
                    <table className="table">
                        <thead><tr><th>Year</th><th className="num">Pos</th><th className="num">Pts</th><th className="num">Wins</th><th className="num">Pod</th><th className="num">Races</th><th className="num">Best</th></tr></thead>
                        <tbody>
                        {team.seasons.map(s => (
                            <tr key={s.year}>
                                <td><Link to={`/seasons/${s.year}`}>{s.year}</Link></td>
                                <td className="num">{s.standingPosition}</td><td className="num">{s.points}</td>
                                <td className="num">{s.wins}</td><td className="num">{s.podiums}</td>
                                <td className="num">{s.races}</td><td className="num">{s.bestFinish ?? '—'}</td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>
            </div>
        </section>
    )
}