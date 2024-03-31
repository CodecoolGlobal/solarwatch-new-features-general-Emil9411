import React from 'react'
import ReactDOM from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import App from './App.jsx'
import Login from './pages/Login.jsx'
import Registration from './pages/Registration.jsx'
import SolarWatch from './pages/SolarWatch.jsx'
import Profile from './pages/Profile.jsx';
import Admin from './pages/Admin.jsx';
import './design/index.css'

const router = createBrowserRouter([
  {
    path: '/',
    element: <App />,
    children: [
      { path: '/login', element: <Login /> },
      { path: '/register', element: <Registration /> },
      { path: '/solarwatch', element: <SolarWatch /> },
      { path: '/profile', element: <Profile /> },
      { path: '/admin', element: <Admin /> },
    ],
  },
]);

const root = ReactDOM.createRoot(document.getElementById('root'));

root.render(
  <React.StrictMode>
    <RouterProvider router={router}/>
  </React.StrictMode>,
)
