import React, { useEffect, useState } from "react";

function Profile() {
  const [user, setUser] = useState(null);
  const [email, setEmail] = useState(null);
  const [city, setCity] = useState(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    async function fetchData() {
      try {
        const response = await fetch("/api/Auth/getuser", {
          method: "GET",
          credentials: "include",
          headers: {
            "Content-Type": "application/json",
          },
        });
        const data = await response.json();
        if (data) {
          setUser(data.userName);
          setEmail(data.email);
          setCity(data.city);
        }
        console.log(data);
      } catch (error) {
        console.log("Error", error);
      }
    }
    fetchData();
  }, []);

  return (
    <div>
      <h1>Welcome {user}!</h1>
      <h3>Get SolarWatch Data Of Your City!</h3>
      <button>SolarWatch</button>
    </div>
  );
}

export default Profile;
