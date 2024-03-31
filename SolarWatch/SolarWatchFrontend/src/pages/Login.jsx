import React from 'react';
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "../design/index.css";

function Login() {
    const [emailOrUserName, setEmailOrUserName] = useState("");
    const [password, setPassword] = useState("");
    const navigate = useNavigate();

    async function handleSubmit(event) {
        event.preventDefault();
        try {
          const response = await fetch("/api/Auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ emailOrUserName, password }),
          });
          if (!response.ok) {
            throw new Error("Invalid credentials");
          }
          const data = await response.json();
          setPassword("");
          setEmailOrUserName("");
          navigate("/");
        } catch (error) {
          alert(error.message);
          console.error(error);
        }
      }

    return (
        <div>
            <h1>Login</h1>
            <form onSubmit={handleSubmit}>
          <label>Username/Email:</label>
          <br />

          <input
            type="text"
            name="emailOrUserName"
            value={emailOrUserName}
            onChange={(e) => setEmailOrUserName(e.target.value)}
          />
          <br />

          <label>Password:</label>
          <br />
          <input
            type="password"
            name="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          <br />
          <button type="submit">Login</button>
          <button onClick={() => navigate("/")}>Cancel</button>
        </form>
        </div>
    );
}

export default Login;