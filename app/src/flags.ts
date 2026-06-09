// Driver / constructor nationality (demonym) -> ISO 3166-1 alpha-2
const nationalityToIso: Record<string, string> = {
    British: 'gb', English: 'gb', Scottish: 'gb', Welsh: 'gb',
    German: 'de', 'East German': 'de', Italian: 'it', French: 'fr', Spanish: 'es',
    Dutch: 'nl', Finnish: 'fi', Austrian: 'at', Belgian: 'be', Swiss: 'ch',
    Swedish: 'se', Danish: 'dk', Norwegian: 'no', Irish: 'ie', Portuguese: 'pt',
    Polish: 'pl', Czech: 'cz', Hungarian: 'hu', Russian: 'ru', Monegasque: 'mc',
    American: 'us', Canadian: 'ca', Mexican: 'mx', Brazilian: 'br', Argentine: 'ar',
    Argentinian: 'ar', Colombian: 'co', Venezuelan: 've', Chilean: 'cl', Uruguayan: 'uy',
    Australian: 'au', 'New Zealander': 'nz', Japanese: 'jp', Thai: 'th', Chinese: 'cn',
    Indian: 'in', Indonesian: 'id', Malaysian: 'my', 'South African': 'za',
    Rhodesian: 'zw', Liechtensteiner: 'li',
}

// Circuit country name -> ISO 3166-1 alpha-2
const countryToIso: Record<string, string> = {
    Australia: 'au', Bahrain: 'bh', 'Saudi Arabia': 'sa', Japan: 'jp', China: 'cn',
    USA: 'us', 'United States': 'us', Italy: 'it', Monaco: 'mc', Canada: 'ca',
    Spain: 'es', Austria: 'at', UK: 'gb', 'United Kingdom': 'gb', Hungary: 'hu',
    Belgium: 'be', Netherlands: 'nl', Azerbaijan: 'az', Singapore: 'sg', Mexico: 'mx',
    Brazil: 'br', UAE: 'ae', 'United Arab Emirates': 'ae', Qatar: 'qa', France: 'fr',
    Portugal: 'pt', Turkey: 'tr', Germany: 'de', Russia: 'ru', Vietnam: 'vn',
    Korea: 'kr', 'South Korea': 'kr', India: 'in', Malaysia: 'my', Argentina: 'ar',
    Morocco: 'ma', Sweden: 'se', Switzerland: 'ch', 'South Africa': 'za',
}

export const isoForNationality = (n?: string | null) =>
    (n && nationalityToIso[n.trim()]) || null
export const isoForCountry = (c?: string | null) =>
    (c && countryToIso[c.trim()]) || null