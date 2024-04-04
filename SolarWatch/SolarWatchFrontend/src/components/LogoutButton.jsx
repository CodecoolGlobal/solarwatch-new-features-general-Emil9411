import React, { useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import MessageModal from "./MessageModal";

function LogoutButton() {
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [showErrorModal, setShowErrorModal] = useState(false);

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
      if (!response.ok) {
        throw new Error("Logout failed");
      } else {
        setShowSuccessModal(true);
      }
    } catch (error) {
      setShowErrorModal(true);
      console.error("Logout failed", error);
    }
  }

  function handleCloseSuccessModal() {
    setShowSuccessModal(false);
    location.pathname === "/" ? window.location.reload() : navigate("/");
  }

  function handleCloseErrorModal() {
    setShowErrorModal(false);
  }

  return (
    <>
      <button onClick={handleLogout}>Logout</button>
      <MessageModal isOpen={showSuccessModal} message="Logout successful" handleClose={handleCloseSuccessModal} />
      <MessageModal isOpen={showErrorModal} message="Logout failed. Please try again." handleClose={handleCloseErrorModal} />
    </>
  );
}

export default LogoutButton;
