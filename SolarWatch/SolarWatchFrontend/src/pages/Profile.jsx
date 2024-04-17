import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import MessageModal from "../components/MessageModal";
import "../design/index.css";

function Profile() {
  const [id, setId] = useState("");
  const [userName, setUserName] = useState("");
  const [city, setCity] = useState("");

  const [editButtonPressed, setEditButtonPressed] = useState(false);
  const [deleteAccountButtonPressed, setDeleteAccountButtonPressed] = useState(false);

  const [editButtonText, setEditButtonText] = useState("Edit your profile");

  const [showProfileSavedModal, setShowProfileSavedModal] = useState(false);
  const [showProfileSaveFailedModal, setShowProfileSaveFailedModal] = useState(false);
  const [showDeleteConfirmationModal, setShowDeleteConfirmationModal] = useState(false);
  const [showAccountDeletedModal, setShowAccountDeletedModal] = useState(false);
  const [showAccountDeleteFailedModal, setShowAccountDeleteFailedModal] = useState(false);

  const navigate = useNavigate();

  useEffect(() => {
    async function fetchData() {
      let email = "";
      try {
        const cookieResponse = await fetch("/api/Auth/whoami", {
          method: "GET",
          credentials: "include",
          headers: {
            "Content-Type": "application/json",
          },
        });
        const cookieData = await cookieResponse.json();
        if (cookieData) {
          email = cookieData.email;
        } else {
          throw new Error("No user logged in");
        }
        const getUserResponse = await fetch(`/api/User/getbyuserdata/${email}`, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
          },
        });
        const userData = await getUserResponse.json();
        if (userData) {
          setId(userData.id || "");
          setUserName(userData.userName || "");
          setCity(userData.city || "");
        } else {
          throw new Error("No user found");
        }
      } catch (error) {
        console.log("Error", error);
      }
    }
    fetchData();
  }, []);

  useEffect(() => {
    if (editButtonPressed) {
      setEditButtonText("Cancel editing");
    } else {
      setEditButtonText("Edit your profile");
    }
  }, [editButtonPressed]);

  async function handleEditProfile() {
    try {
      const response = await fetch(`/api/User/update/${id}`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          userName: userName,
          city: city,
          phoneNumber: phone,
        }),
      });
      if (!response.ok) {
        setShowProfileSaveFailedModal(true);
        throw new Error("Failed to update user");
      } else {
        setUserName(userName);
        setCity(city);
        setShowProfileSavedModal(true);
      }
    } catch (error) {
      console.log("Error", error);
    }
  }

  async function handleAccountDeletion() {
    console.log("Deleting account");
    try {
      const response = await fetch(`/api/User/delete/${id}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      });
      if (!response.ok) {
        setShowDeleteConfirmationModal(false);
        setShowAccountDeleteFailedModal(true);
        throw new Error("Failed to delete user");
      } else {
        setShowDeleteConfirmationModal(false);
        setShowAccountDeletedModal(true);
      }
    } catch (error) {
      console.log("Error", error);
    }
  }

  function handleEditButton() {
    setEditButtonPressed(!editButtonPressed);
  }

  function handleProfileSavedModal() {
    setShowProfileSavedModal(false);
    setEditButtonPressed(false);
  }

  async function handleAccountDeletedModal() {
    setShowAccountDeletedModal(false);
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
        navigate("/");
        window.location.reload();
      }
    } catch (error) {
      setShowErrorModal(true);
      console.error("Logout failed", error);
    }
  }

  function handleDeleteAccount() {
    setDeleteAccountButtonPressed(true);
    setShowDeleteConfirmationModal(true);
  }

  function handleDeleteConfirmationCloseModal() {
    setDeleteAccountButtonPressed(false);
    setShowDeleteConfirmationModal(false);
  }

  return (
    <div>
      <h1>Welcome {userName}!</h1>
      <label>Username:</label>
      <br />
      <input
        type="text"
        value={userName}
        onChange={(e) => setUserName(e.target.value)}
        disabled={!editButtonPressed}
      />
      <br />
      <label>City:</label>
      <br />
      <input
        type="text"
        value={city}
        onChange={(e) => setCity(e.target.value)}
        disabled={!editButtonPressed}
      />
      <br />
      {editButtonPressed && <button onClick={handleEditProfile}>Save changes</button>}
      <br />
      <button onClick={handleEditButton} disabled={deleteAccountButtonPressed}>
        {editButtonText}
      </button>
      <br />
      <button onClick={handleDeleteAccount} disabled={editButtonPressed}>
        Delete your account
      </button>
      <MessageModal
        isOpen={showProfileSavedModal}
        message="Profile saved successfully!"
        handleClose={handleProfileSavedModal}
      />
      <MessageModal
        isOpen={showProfileSaveFailedModal}
        message="Failed to save profile!"
        handleClose={() => setShowProfileSaveFailedModal(false)}
      />
      <MessageModal
        isOpen={showDeleteConfirmationModal}
        message="Are you sure you want to delete your account?"
        handleClose={handleDeleteConfirmationCloseModal}
        handleConfirm={handleAccountDeletion}
      />
      <MessageModal
        isOpen={showAccountDeletedModal}
        message="Your account has been deleted!"
        handleClose={handleAccountDeletedModal}
      />
      <MessageModal
        isOpen={showAccountDeleteFailedModal}
        message="Failed to delete your account!"
        handleClose={() => setShowAccountDeleteFailedModal(false)}
      />
    </div>
  );
}

export default Profile;
