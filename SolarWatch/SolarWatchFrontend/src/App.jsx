import React from "react";
import { Outlet, Link, useLocation } from "react-router-dom";

import "./App.css";
import Logo from "./img/sun.png";

function App() {
  return (
    <div className="App">
      <div className="container">
        <Link to="/">
          <img className="logo" src={Logo} alt="SolarWatch" />
        </Link>
        <Link to="/login">
          <button>Login</button>
        </Link>
        <Link to="/register">
          <button>Register</button>
        </Link>
        <Link to="/solarwatch">
          <button>SolarWatch</button>
        </Link>
        </div>
      <Outlet />
    </div>
  );
}

export default App;
