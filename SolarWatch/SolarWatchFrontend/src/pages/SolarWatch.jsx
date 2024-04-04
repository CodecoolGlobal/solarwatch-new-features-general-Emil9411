import React, { useState, useEffect } from "react";
import OneRowTable from "../components/OneRowTable";
import "../design/index.css";

function SolarWatch() {
  const today = new Date().toISOString().slice(0, 10);
  const [data, setData] = useState(null);
  const [loaded, setLoaded] = useState(false);
  const [city, setCity] = useState("");
  const [userEmail, setUserEmail] = useState("");
  const [userCity, setUserCity] = useState("");
  const [userCityUsed, setUserCityUsed] = useState(false);
  const [date, setDate] = useState(today);
  const [loadingScreen, setLoadingScreen] = useState(false);

  useEffect(() => {
    async function fetchData() {
      let email = "";
      try {
        const getUserResponse = await fetch("/api/Auth/whoami", {
          method: "GET",
          credentials: "include",
          headers: {
            "Content-Type": "application/json",
          },
        });
        const getUserData = await getUserResponse.json();
        console.log(getUserData);
        if (getUserData) {
          setUserEmail(getUserData.email);
          email = getUserData.email;
        } else {
          throw new Error("User not found");
        }
        const getUserCityResponse = await fetch(`/api/User/getbyuserdata/${email}`, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
          },
        });
        const getUserCityData = await getUserCityResponse.json();
        console.log(getUserCityData);
        if (getUserCityData) {
          setUserCity(getUserCityData.city);
        } else {
          throw new Error("City not found");
        }
      } catch (error) {
        console.log("Error", error);
      }
    }
    fetchData();
  }, []);

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
      setData(data);
      setLoaded(true);
      setLoadingScreen(false);
      setUserCityUsed(false);
    } catch (e) {
      console.error(e);
    }
  }

  function handleClear() {
    setData(null);
    setLoaded(false);
    setCity("");
    setDate(today);
    setUserCityUsed(false);
  }

  function handleCityChange() {
    setCity(userCity);
    setUserCityUsed(true);
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
        <>
          <button onClick={handleCityChange} disabled={userCityUsed}>Use my city</button>
          <button onClick={fetchData}>Get SolarWatch Data</button>
        </>
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
