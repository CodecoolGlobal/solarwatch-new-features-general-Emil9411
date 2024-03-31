import React from "react";
import { useState } from "react";
import OneRowTable from "../components/OneRowTable";
import "../design/index.css";

function SolarWatch() {
  const [data, setData] = useState(null);
  const [loaded, setLoaded] = useState(false);
  const [city, setCity] = useState("");
  const [date, setDate] = useState("");

  async function fetchData() {
    try {
      const response = await fetch(`/api/SW/getdata/${city}/${date}`, {
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

  function handleClear() {
    setData(null);
    setLoaded(false);
    setCity("");
    setDate("");
  }

  return (
    <div>
      <h1>SolarWatch</h1>
      <label>City:</label>
      <br />
      <input
        type="text"
        value={city}
        onChange={(e) => setCity(e.target.value)}
        disabled={loaded}
      ></input>
      <br />
      <label>Date:</label>
      <br />
      <input
        type="date"
        value={date}
        onChange={(e) => setDate(e.target.value)}
        disabled={loaded}
      ></input>
      <br />
      <br />
      {loaded === false ? (
        <button onClick={fetchData}>Get SolarWatch Data</button>
      ) : (
        <>
          <OneRowTable dataObject={data} />
          <button onClick={handleClear}>Clear</button>
        </>
      )}
    </div>
  );
}

export default SolarWatch;
