import React from "react";
import { useNavigate, useLocation } from "react-router-dom";

function LogoutButton() {
  const navigate = useNavigate();
  const location = useLocation();

  async function handleLogout() {
    try {
      const response = await fetch("/api/Auth/logout", {
        method: "POST",
        credentials: "include",
        headers: {
          "Content-Type": "application/json",
        },
      });
      if (response.ok) {
        if (location.pathname !== "/") {
          navigate("/");
          window.location.reload();
        } else {
          window.location.reload();
        }
      }
    } catch (error) {
      console.error("Logout failed", error);
    }
  }

  return <button onClick={handleLogout}>Logout</button>;
}

export default LogoutButton;
