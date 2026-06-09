export function Stat({ label, value }: { label: string; value: number | string }) {
    return (
        <div className="stat">
            <div className="stat__num">{value}</div>
            <div className="stat__label">{label}</div>
        </div>
    )
}