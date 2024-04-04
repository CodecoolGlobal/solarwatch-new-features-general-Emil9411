import React, { useState } from "react";
import AdminTable from "../components/AdminTable";
import SolarWatchUpdateForm from "../components/forms/SolarWatchUpdateForm";
import CityDataUpdateForm from "../components/forms/CityDataUpdateForm";
import UserDataUpdateForm from "../components/forms/UserDataUpdateForm";
import "../design/index.css";

function Admin() {
  const [cityData, setCityData] = useState([]);
  const [solarWatchData, setSolarWatchData] = useState([]);
  const [userData, setUserData] = useState([]);

  const [cityToUpdate, setCityToUpdate] = useState({});
  const [solarWatchToUpdate, setSolarWatchToUpdate] = useState({});
  const [userToUpdate, setUserToUpdate] = useState({});

  const [cityUpdateForm, setCityUpdateForm] = useState(false);
  const [solarWatchUpdateForm, setSolarWatchUpdateForm] = useState(false);
  const [userUpdateForm, setUserUpdateForm] = useState(false);

  const [showSolarWatchTable, setShowSolarWatchTable] = useState(false);
  const [showCityDataTable, setShowCityDataTable] = useState(false);
  const [showUserDataTable, setShowUserDataTable] = useState(false);

  const [loadingScreen, setLoadingScreen] = useState(false);

  // City data fucntions
  async function fetchCityData() {
    try {
      setLoadingScreen(true);
      const response = await fetch(`/api/Location/getall`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });
      const data = await response.json();
      console.log(data);
      setCityData(data);
      setLoadingScreen(false);
    } catch (e) {
      console.error(e);
    }
  }

  async function handleCityDelete(id) {
    try {
      await fetch(`/api/Location/delete/${id}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      });
      setCityData(cityData.filter((row) => row.id !== id));
      console.log("Deleted");
    } catch (e) {
      console.error(e);
    }
  }

  async function handleCityUpdate(id, data) {
    try {
      await fetch(`/api/Location/update/${id}`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });
      setCityData(cityData.map((row) => (row.id === id ? { ...row, ...data } : row)));
      console.log("Updated");
    } catch (e) {
      console.error(e);
    }
  }

  function handleCityDataToUpdate(data) {
    setCityToUpdate(data);
    setCityUpdateForm(true);
    setShowCityDataTable(false);
  }

  function clearCityData() {
    setCityToUpdate({});
    setCityUpdateForm(false);
    setShowCityDataTable(true);
  }

  // SolarWatch data functions
  async function fetchSolarWatchData() {
    try {
      setLoadingScreen(true);
      const response = await fetch(`/api/SW/getall`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });
      const data = await response.json();
      console.log(data);
      setSolarWatchData(data);
      setLoadingScreen(false);
    } catch (e) {
      console.error(e);
    }
  }

  async function handleSolarWatchDelete(id) {
    try {
      await fetch(`/api/SW/delete/${id}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      });
      setSolarWatchData(solarWatchData.filter((row) => row.id !== id));
      console.log("Deleted");
    } catch (e) {
      console.error(e);
    }
  }

  async function handleSolarWatchUpdate(id, data) {
    try {
      await fetch(`/api/SW/update/${id}`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });
      setSolarWatchData(
        solarWatchData.map((row) => (row.id === id ? { ...row, ...data } : row))
      );
      console.log("Updated");
    } catch (e) {
      console.error(e);
    }
  }

  function handleSolarWatchDataToUpdate(data) {
    setSolarWatchToUpdate(data);
    setSolarWatchUpdateForm(true);
    setShowSolarWatchTable(false);
  }

  function clearSolarWatchData() {
    setSolarWatchToUpdate({});
    setSolarWatchUpdateForm(false);
    setShowSolarWatchTable(true);
  }

  // User data functions
  async function fetchUserData() {
    try {
      setLoadingScreen(true);
      const response = await fetch(`/api/User/getall`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });
      const data = await response.json();
      console.log(data);
      setUserData(data);
      setLoadingScreen(false);
    } catch (e) {
      console.error(e);
    }
  }

  async function handleUserDelete(id) {
    try {
      await fetch(`/api/User/delete/${id}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      });
      setUserData(userData.filter((row) => row.id !== id));
      console.log("Deleted");
    } catch (e) {
      console.error(e);
    }
  }

  async function handleUserUpdate(id, data) {
    try {
      await fetch(`/api/User/update/${id}`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });
      setUserData(userData.map((row) => (row.id === id ? { ...row, ...data } : row)));
      console.log("Updated");
    } catch (e) {
      console.error(e);
    }
  }

  function handleUserDataToUpdate(data) {
    setUserToUpdate(data);
    setUserUpdateForm(true);
    setShowUserDataTable(false);
  }

  function clearUserData() {
    setUserToUpdate({});
    setUserUpdateForm(false);
    setShowUserDataTable(true);
  }

  return (
    <div>
      <h1>Admin site</h1>
      <br />
      <div>
        <button
          onClick={() => {
            setShowCityDataTable(true);
            setShowSolarWatchTable(false);
            setShowUserDataTable(false);
            fetchCityData();
          }}
          disabled={showCityDataTable}
        >
          Show City Data
        </button>
        <button
          onClick={() => {
            setShowSolarWatchTable(true);
            setShowCityDataTable(false);
            setShowUserDataTable(false);
            fetchSolarWatchData();
          }}
          disabled={showSolarWatchTable}
        >
          Show SolarWatch Data
        </button>
        <button
          onClick={() => {
            setShowUserDataTable(true);
            setShowCityDataTable(false);
            setShowSolarWatchTable(false);
            fetchUserData();
          }}
          disabled={showUserDataTable}
        >
          Show User Data
        </button>
      </div>
      <br />
      {showCityDataTable &&
      !showSolarWatchTable &&
      !showUserDataTable &&
      loadingScreen &&
      !cityUpdateForm &&
      !solarWatchUpdateForm &&
      !userUpdateForm ? (
        <h2>Loading City Data...</h2>
      ) : !showCityDataTable &&
        showSolarWatchTable &&
        !showUserDataTable &&
        loadingScreen &&
        !cityUpdateForm &&
        !solarWatchUpdateForm &&
        !userUpdateForm ? (
        <h2>Loading SolarWatch Data...</h2>
      ) : !showCityDataTable &&
        !showSolarWatchTable &&
        showUserDataTable &&
        loadingScreen &&
        !cityUpdateForm &&
        !solarWatchUpdateForm &&
        !userUpdateForm ? (
        <h2>Loading User Data...</h2>
      ) : showCityDataTable &&
        !showSolarWatchTable &&
        !showUserDataTable &&
        !loadingScreen &&
        !cityUpdateForm &&
        !solarWatchUpdateForm &&
        !userUpdateForm ? (
        <AdminTable
          data={cityData}
          handleDelete={handleCityDelete}
          handleUpdate={handleCityDataToUpdate}
        />
      ) : !showCityDataTable &&
        showSolarWatchTable &&
        !showUserDataTable &&
        !loadingScreen &&
        !cityUpdateForm &&
        !solarWatchUpdateForm &&
        !userUpdateForm ? (
        <AdminTable
          data={solarWatchData}
          handleDelete={handleSolarWatchDelete}
          handleUpdate={handleSolarWatchDataToUpdate}
        />
      ) : !showCityDataTable &&
        !showSolarWatchTable &&
        showUserDataTable &&
        !loadingScreen &&
        !cityUpdateForm &&
        !solarWatchUpdateForm &&
        !userUpdateForm ? (
        <AdminTable
          data={userData}
          handleDelete={handleUserDelete}
          handleUpdate={handleUserDataToUpdate}
        />
      ) : cityUpdateForm && !solarWatchUpdateForm && !userUpdateForm ? (
        <CityDataUpdateForm
          data={cityToUpdate}
          handleUpdate={handleCityUpdate}
          clearData={clearCityData}
        />
      ) : !cityUpdateForm && solarWatchUpdateForm && !userUpdateForm ? (
        <SolarWatchUpdateForm
          data={solarWatchToUpdate}
          handleUpdate={handleSolarWatchUpdate}
          clearData={clearSolarWatchData}
        />
      ) : !cityUpdateForm && !solarWatchUpdateForm && userUpdateForm ? (
        <UserDataUpdateForm
          data={userToUpdate}
          handleUpdate={handleUserUpdate}
          clearData={clearUserData}
        />
      ) : (
        <h2>Waiting for instructions...</h2>
      )}
    </div>
  );
}

export default Admin;
