import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import MessageModal from "../components/MessageModal";
import "../design/index.css";

function Registration() {
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [city, setCity] = useState("");
  const [showExistingUserModal, setShowExistingUserModal] = useState(false);
  const [showPasswordMismatchModal, setShowPasswordMismatchModal] = useState(false);
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [formSubmitted, setFormSubmitted] = useState(false);
  const navigate = useNavigate();

  async function handleSubmit(event) {
    event.preventDefault();
    setFormSubmitted(true);
    if (userName === "" || email === "" || password === "" || city === "") {
      return; // Don't submit the form if inputs are empty
    }
    if (password !== confirmPassword) {
        setShowPasswordMismatchModal(true);
        return; // Don't submit the form if passwords don't match
    }
    const user = {
      email: email,
      userName: userName,
      password: password,
      city: city,
    };
    try {
      const response = await fetch("/api/Auth/register", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(user),
      });
      if (!response.ok) {
        throw new Error("Registration failed!");
      }
      setShowExistingUserModal(false);
      setShowPasswordMismatchModal(false);
      setShowSuccessModal(true);
    } catch (error) {
        setShowExistingUserModal(true);
        setShowPasswordMismatchModal(false);
        setShowSuccessModal(false);
      console.error(error);
    }
  }

  function handleCloseModals() {
    if (showExistingUserModal || showPasswordMismatchModal) {
      // Don't navigate if either error modal is showing
      setShowExistingUserModal(false);
      setShowPasswordMismatchModal(false);
    } else {
      // Navigate only if the successful registration modal is showing
      setShowSuccessModal(false);
      navigate("/login");
    }
  }

  return (
    <div className="registration">
      <form className="registrationForm" onSubmit={handleSubmit}>
        <label>Username:</label>
        <br />
        {(formSubmitted && userName === "") && <p style={{ color: "red" }}>Please enter your username</p>}
        <input
          type="text"
          name="username"
          onChange={(e) => setUserName(e.target.value)}
        ></input>
        <br />
        <label>Email:</label>
        <br />
        {(formSubmitted && email === "") && <p style={{ color: "red" }}>Please enter your email</p>}
        <input type="email" name="email" onChange={(e) => setEmail(e.target.value)}></input>
        <br />
        <label>Password:</label>
        <br />
        {(formSubmitted && password === "") && <p style={{ color: "red" }}>Please enter your password</p>}
        <input
          type="password"
          name="password"
          onChange={(e) => setPassword(e.target.value)}
        ></input>
        <br />
        <label>Confirm Password:</label>
        <br />
        {(formSubmitted && confirmPassword === "") && <p style={{ color: "red" }}>Please confirm your password</p>}
        <input
          type="password"
          name="password"
          onChange={(e) => setConfirmPassword(e.target.value)}
        ></input>
        <br />
        <label>City:</label>
        <br />
        {(formSubmitted && city === "") && <p style={{ color: "red" }}>Please enter your city</p>}
        <input type="text" name="city" onChange={(e) => setCity(e.target.value)}></input>
        <br />
        <button type="submit">Register</button>
        <button type="button" onClick={() => navigate("/")}>
          Cancel
        </button>
      </form>
      <MessageModal isOpen={showExistingUserModal} message="Username or email already exists. Please try again." handleClose={handleCloseModals} />
      <MessageModal isOpen={showPasswordMismatchModal} message="Passwords do not match. Please try again." handleClose={handleCloseModals} />
      <MessageModal isOpen={showSuccessModal} message="Registration successful" handleClose={handleCloseModals} />
    </div>
  );
}

export default Registration;
