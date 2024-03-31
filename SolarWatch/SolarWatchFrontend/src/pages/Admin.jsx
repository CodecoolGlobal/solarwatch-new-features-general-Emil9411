import React from "react";
import { useState } from "react";
import "../design/index.css";
import AdminTable from "../components/AdminTable";

function Admin() {
  const [data, setData] = useState(null);
  const [loaded, setLoaded] = useState(false);

  async function fetchData() {
    try {
      const response = await fetch(`/api/SW/getall`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });
      const data = await response.json();
      console.log(data);
      setData(data);
      setLoaded(true);
    } catch (e) {
      console.error(e);
    }
  }

  return (
    <div>
      <h1>Admin site</h1>
      <br />
      {loaded === true ? (
        <AdminTable data={data} />
      ) : (
        <button onClick={fetchData}>Get All SolarWatch Data</button>
      )}
    </div>
  );
}

export default Admin;
