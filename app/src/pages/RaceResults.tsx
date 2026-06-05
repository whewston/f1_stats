import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { api, type RaceResults as RaceResultsType } from '../api'

export default function RaceResults() {
    const { year, round } = useParams()
    const [data, setData] = useState<RaceResultsType | null>(null)
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        setLoading(true)
        api.results(Number(year), Number(round)).then(setData).finally(() => setLoading(false))
    }, [year, round])

    if (loading) return <p>Loading…</p>
    if (!data) return <p>No results found.</p>

    return (
        <section>
            <h2>{data.raceName}</h2>
            <p style={{ color: '#555' }}>{data.circuitName} · {data.date}</p>
            {data.results.length === 0 ? <p>No results yet — this race hasn’t run.</p> : (
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead><tr style={{ textAlign: 'left', borderBottom: '2px solid #333' }}>
                        <th>Pos</th><th>Driver</th><th>Constructor</th><th>Grid</th><th>Pts</th><th>Status</th>
                    </tr></thead>
                    <tbody>
                    {data.results.map(r => (
                        <tr key={r.driver} style={{ borderBottom: '1px solid #eee' }}>
                            <td>{r.positionText}</td><td>{r.driver}</td><td>{r.constructor}</td>
                            <td>{r.grid ?? '—'}</td><td>{r.points}</td><td>{r.status}</td>
                        </tr>
                    ))}
                    </tbody>
                </table>
            )}
        </section>
    )
}