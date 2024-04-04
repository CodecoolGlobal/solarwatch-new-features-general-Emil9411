import React, { useEffect, useState } from "react";
import { Outlet, Link, useLocation } from "react-router-dom";
import LogoutButton from "./components/LogoutButton";
import Logo from "./img/sun.png";
import "./design/index.css";

function App() {
  const [user, setUser] = useState(null);
  const location = useLocation();

  useEffect(() => {
    async function fetchData() {
      try {
        const response = await fetch("/api/Auth/whoami", {
          method: "GET",
          credentials: "include",
          headers: {
            "Content-Type": "application/json",
          },
        });
        const data = await response.json();
        if (data) {
          setUser(data.userName);
        }
      } catch (error) {
        console.log("Error", error);
      }
    }
    fetchData();
  }, [location.pathname]);

  return (
    <div className="App">
      <div className="container">
        <Link to="/">
          <img className="logo" src={Logo} alt="SolarWatch" />
        </Link>
        {user === null ? (
          <>
            <Link to="/login">
              <button>Login</button>
            </Link>
            <Link to="/register">
              <button>Register</button>
            </Link>
          </>
        ) : user !== "admin" ? (
          <>
            <Link to="/profile">
              <button>Profile</button>
            </Link>
            <Link to="/solarwatch">
              <button>SolarWatch</button>
            </Link>
            <LogoutButton />
          </>
        ) : (
          <>
            <Link to="/admin">
              <button>Admin</button>
            </Link>
            <Link to="/solarwatch">
              <button>SolarWatch</button>
            </Link>
            <LogoutButton />
          </>
        )}
      </div>
      <Outlet />
    </div>
  );
}

export default App;
