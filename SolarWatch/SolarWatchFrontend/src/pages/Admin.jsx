import React from "react";
import { useState, useEffect } from "react";
import "../design/index.css";
import AdminTable from "../components/AdminTable";
import AdminUpdateForm from "../components/AdminUpdateForm";

function Admin() {
  const [cityData, setCityData] = useState([]);
  const [cityToUpdate, setCityToUpdate] = useState({});
  const [updateFrom, setUpdateFrom] = useState(false);

  useEffect(() => {
    async function fetchData() {
      const response = await fetch(`/api/SW/getall`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });
      const data = await response.json();
      setCityData(data);
    }
    fetchData();
  }, []);

  async function handleDelete(id) {
    try {
      await fetch(`/api/SW/delete/${id}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      });
      setCityData(cityData.filter((row) => row.id !== id));
    } catch (e) {
      console.error(e);
    }
  }

  async function handleUpdate(id, data) {
    try {
      await fetch(`/api/SW/update/${id}`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });
      setCityData(
        cityData.map((row) => (row.id === id ? { ...row, ...data } : row))
      );
    } catch (e) {
      console.error(e);
    }
  }

  function handleDataToUpdate(data) {
    setCityToUpdate(data);
    setUpdateFrom(true);
  }

  function clearData() {
    setCityToUpdate({});
    setUpdateFrom(false);
  }

  return (
    <div>
      <h1>Admin site</h1>
      <br />
      {cityData.length !== 0 && updateFrom === false ? (
        <AdminTable data={cityData} onDelete={handleDelete} onUpdate={handleDataToUpdate} />
      ) : updateFrom === true ? (
        <AdminUpdateForm
          data={cityToUpdate}
          onUpdate={handleUpdate}
          clearData={clearData}
        />
      ) : (
        <h2>No data available</h2>
      )}
    </div>
  );
}

export default Admin;
