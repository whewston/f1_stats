import React from 'react'
import ReactDOM from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import App from './App'
import Home from './pages/Home'
import SeasonRaces from './pages/SeasonRaces'
import DriverPage from './pages/DriverPage'
import ConstructorPage from './pages/ConstructorPage'
import RacePage from './pages/RacePage'
import './index.css'
import 'flag-icons/css/flag-icons.min.css'


const router = createBrowserRouter([
    { path: '/', element: <App />, children: [
            { index: true, element: <Home /> },
            { path: 'seasons/:year', element: <SeasonRaces /> },
            { path: 'seasons/:year/races/:round', element: <RacePage /> },
            { path: 'drivers/:driverId', element: <DriverPage /> },
            { path: 'constructors/:constructorId', element: <ConstructorPage /> },
        ],},
])

ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode><RouterProvider router={router} /></React.StrictMode>,
)