import React from 'react'
import ReactDOM from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import App from './App'
import Home from './pages/Home'
import SeasonRaces from './pages/SeasonRaces'
import RaceResults from './pages/RaceResults'
import './index.css'

const router = createBrowserRouter([
    { path: '/', element: <App />, children: [
            { index: true, element: <Home /> },
            { path: 'seasons/:year', element: <SeasonRaces /> },
            { path: 'seasons/:year/races/:round', element: <RaceResults /> },
        ]},
])

ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode><RouterProvider router={router} /></React.StrictMode>,
)