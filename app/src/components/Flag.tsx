import { isoForCountry, isoForNationality } from '../flags'

type Props = { country?: string | null; nationality?: string | null; title?: string }

export function Flag({ country, nationality, title }: Props) {
    const iso = country != null ? isoForCountry(country) : isoForNationality(nationality)
    if (!iso) return null
    const label = title ?? country ?? nationality ?? ''
    return <span className={`fi fi-${iso} flag`} title={label} aria-label={label} role="img" />
}