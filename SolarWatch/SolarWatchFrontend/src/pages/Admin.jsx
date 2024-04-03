import React from "react";
import { useState } from "react";
import "../design/index.css";
import AdminTable from "../components/AdminTable";
import SolarWatchUpdateForm from "../components/SolarWatchUpdateForm";
import CityDataUpdateForm from "../components/CityDataUpdateForm";

function Admin() {
  const [cityData, setCityData] = useState([]);
  const [solarWatchData, setSolarWatchData] = useState([]);

  const [cityToUpdate, setCityToUpdate] = useState({});
  const [solarWatchToUpdate, setSolarWatchToUpdate] = useState({});

  const [cityUpdateForm, setCityUpdateForm] = useState(false);
  const [solarWatchUpdateForm, setSolarWatchUpdateForm] = useState(false);

  const [showSolarWatchTable, setShowSolarWatchTable] = useState(false);
  const [showCityDataTable, setShowCityDataTable] = useState(false);

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

  return (
    <div>
      <h1>Admin site</h1>
      <br />
      <div>
        <button
          onClick={() => {
            setShowCityDataTable(true);
            setShowSolarWatchTable(false);
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
            fetchSolarWatchData();
          }}
          disabled={showSolarWatchTable}
        >
          Show SolarWatch Data
        </button>
      </div>
      <br />
      {showCityDataTable && !showSolarWatchTable && loadingScreen && !cityUpdateForm && !solarWatchUpdateForm ? (
        <h2>Loading City Data...</h2>
      ) : !showCityDataTable && showSolarWatchTable && loadingScreen && !cityUpdateForm && !solarWatchUpdateForm ? (
        <h2>Loading SolarWatch Data...</h2>
      ) : showCityDataTable && !showSolarWatchTable && !cityUpdateForm && !solarWatchUpdateForm ? (
        <AdminTable
          data={cityData}
          handleDelete={handleCityDelete}
          handleUpdate={handleCityDataToUpdate}
        />
      ) : !showCityDataTable && showSolarWatchTable && !cityUpdateForm && !solarWatchUpdateForm ? (
        <AdminTable
          data={solarWatchData}
          handleDelete={handleSolarWatchDelete}
          handleUpdate={handleSolarWatchDataToUpdate}
        />
      ) : cityUpdateForm && !solarWatchUpdateForm ? (
        <CityDataUpdateForm
          data={cityToUpdate}
          handleUpdate={handleCityUpdate}
          clearData={clearCityData}
        />
      ) : !cityUpdateForm && solarWatchUpdateForm ? (
        <SolarWatchUpdateForm
          data={solarWatchToUpdate}
          handleUpdate={handleSolarWatchUpdate}
          clearData={clearSolarWatchData}
        />
      ) : (
        <h2>Waiting for instructions...</h2>
      )}

    </div>
  );
}

export default Admin;
