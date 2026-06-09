const COLORS: Record<string, string> = {
    red_bull: '#3671C6', ferrari: '#E8002D', mercedes: '#27F4D2', mclaren: '#FF8000',
    aston_martin: '#229971', alpine: '#0093CC', williams: '#64C4FF', rb: '#6692FF',
    sauber: '#52E252', haas: '#B6BABD',
}
export const teamColor = (id?: string | null) => (id && COLORS[id]) || '#6b7280'