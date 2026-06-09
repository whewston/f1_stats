export function Pos({ n, text }: { n: number | null; text: string }) {
    const cls = n === 1 ? 'pos pos--p1' : n === 2 ? 'pos pos--p2' : n === 3 ? 'pos pos--p3' : 'pos'
    return <span className={cls}>{text}</span>
}