import React from "react";
import { useState } from "react";
import OneRowTable from "../components/OneRowTable";
import "../design/index.css";

function SolarWatch() {
  const today = new Date().toISOString().slice(0, 10);
  const [data, setData] = useState(null);
  const [loaded, setLoaded] = useState(false);
  const [city, setCity] = useState("");
  const [date, setDate] = useState(today);
  const [loadingScreen, setLoadingScreen] = useState(false);

  async function fetchData() {
    setLoadingScreen(true);
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
      setLoadingScreen(false);
    } catch (e) {
      console.error(e);
    }
  }

  function handleClear() {
    setData(null);
    setLoaded(false);
    setCity("");
    setDate(today);
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
      {loaded === false && loadingScreen === false ? (
        <button onClick={fetchData}>Get SolarWatch Data</button>
      ) : loaded === false && loadingScreen === true ? (
        <p>Loading...</p>
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
