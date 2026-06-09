import { useEffect, useState } from 'react'

type Feature = { bbox?: number[]; geometry: { coordinates: number[][] } }
type FC = { features: Feature[] }

let cache: FC | null = null
async function load(): Promise<FC> {
    if (cache) return cache
    const res = await fetch('/f1-circuits.geojson')
    if (!res.ok) throw new Error('missing geojson')
    cache = await res.json()
    return cache!
}

function nearest(fc: FC, lat: number, lng: number): Feature | null {
    let best: Feature | null = null, bestD = Infinity
    for (const f of fc.features) {
        const b = f.bbox, cs = f.geometry.coordinates
        const clng = b ? (b[0] + b[2]) / 2 : cs.reduce((a, c) => a + c[0], 0) / cs.length
        const clat = b ? (b[1] + b[3]) / 2 : cs.reduce((a, c) => a + c[1], 0) / cs.length
        const d = (clat - lat) ** 2 + (clng - lng) ** 2
        if (d < bestD) { bestD = d; best = f }
    }
    return bestD < 0.25 ? best : null
}

function toPath(coords: number[][], w: number, h: number, pad: number): string {
    const lngs = coords.map(c => c[0]), lats = coords.map(c => c[1])
    const minLng = Math.min(...lngs), maxLng = Math.max(...lngs)
    const minLat = Math.min(...lats), maxLat = Math.max(...lats)
    const s = Math.min((w - 2 * pad) / Math.max(maxLng - minLng, 1e-6),
        (h - 2 * pad) / Math.max(maxLat - minLat, 1e-6))
    const ox = pad + ((w - 2 * pad) - (maxLng - minLng) * s) / 2
    const oy = pad + ((h - 2 * pad) - (maxLat - minLat) * s) / 2
    return coords.map((c, i) =>
        `${i ? 'L' : 'M'}${(ox + (c[0] - minLng) * s).toFixed(1)} ${(oy + (maxLat - c[1]) * s).toFixed(1)}`
    ).join(' ')
}

export function TrackMap({ lat, lng }: { lat?: number | null; lng?: number | null }) {
    const [path, setPath] = useState<string | null>(null)
    const [failed, setFailed] = useState(false)

    useEffect(() => {
        let active = true
        setPath(null); setFailed(false)
        if (lat == null || lng == null) { setFailed(true); return }
        load()
            .then(fc => {
                const f = nearest(fc, lat, lng)
                if (active) f ? setPath(toPath(f.geometry.coordinates, 600, 240, 28)) : setFailed(true)
            })
            .catch(() => active && setFailed(true))
        return () => { active = false }
    }, [lat, lng])

    if (failed) return <div className="empty">Circuit layout unavailable.</div>
    if (!path) return null
    return (
        <svg viewBox="0 0 600 240" width="100%" style={{ display: 'block', height: 'auto' }} role="img" aria-label="Circuit layout">
            <path d={path} fill="none" stroke="var(--accent)" strokeWidth={3} strokeLinejoin="round" strokeLinecap="round" />
        </svg>
    )
}