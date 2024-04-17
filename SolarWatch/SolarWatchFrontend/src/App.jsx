import React, { useEffect, useState } from "react";
import { Outlet, Link, useLocation } from "react-router-dom";
import LogoutButton from "./components/LogoutButton";
import Logo from "./img/sun.png";
import "./design/index.css";
import DarkModeToggle from "react-dark-mode-toggle";

function App() {
  const [user, setUser] = useState(null);
  const [isDarkMode, setIsDarkMode] = useState(() => localStorage.getItem("darkMode") === "true"? true : false);
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

  function toggleDarkMode() {
    setIsDarkMode(!isDarkMode);
  }

  useEffect(() => {
    console.log("Dark mode is", isDarkMode ? "on" : "off");
    const container = document.getElementById("container");
    if (isDarkMode) {
      document.body.className = "dark";
      container.className = "dark";
      localStorage.setItem("darkMode", "true");
    } else {
      document.body.className = "light";
      container.className = "light";
      localStorage.setItem("darkMode", "false");
    }
  }, [isDarkMode]);

  return (
    <div className="App">
      <div id="container">
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
        <DarkModeToggle onChange={toggleDarkMode} checked={isDarkMode} size={70} />
      </div>
      <Outlet />
    </div>
  );
}

export default App;
