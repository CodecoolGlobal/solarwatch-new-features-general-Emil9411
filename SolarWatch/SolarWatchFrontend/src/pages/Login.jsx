import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import MessageModal from "../components/MessageModal";
import "../design/index.css";

function Login() {
  const [emailOrUserName, setEmailOrUserName] = useState("");
  const [password, setPassword] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [showModalError, setShowModalError] = useState(false);
  const [modalMessage, setModalMessage] = useState("");
  const [formSubmitted, setFormSubmitted] = useState(false);

  const navigate = useNavigate();

  async function handleSubmit(event) {
    event.preventDefault();
    setFormSubmitted(true);
    if (emailOrUserName === "" || password === "") {
      return; // Don't submit the form if inputs are empty
    }
    try {
      const response = await fetch("/api/Auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ emailOrUserName, password }),
      });
      if (!response.ok) {
        throw new Error("Invalid credentials");
      }
      setModalMessage("Login successful");
      setShowModal(true);
    } catch (error) {
      setModalMessage("Invalid username/email or password. Please try again.");
      setShowModal(true);
      setShowModalError(true);
      console.error(error);
    }
  }

  function handleCloseModal() {
    setShowModal(false);
    showModalError ? setShowModalError(false) : navigate("/");
  }

  return (
    <div>
      <h1>Login</h1>
      <form onSubmit={handleSubmit}>
        <label>Username/Email:</label>
        <br />
        {(formSubmitted && emailOrUserName === "") && <p style={{ color: "red" }}>Please enter your email or username</p>}
        <input
          type="text"
          name="emailOrUserName"
          value={emailOrUserName}
          onChange={(e) => setEmailOrUserName(e.target.value)}
        />
        <br />
        {(formSubmitted && password === "") && <p style={{ color: "red" }}>Please enter your password</p>}
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
      <MessageModal isOpen={showModal} message={modalMessage} handleClose={handleCloseModal} />
    </div>
  );
}

export default Login;
