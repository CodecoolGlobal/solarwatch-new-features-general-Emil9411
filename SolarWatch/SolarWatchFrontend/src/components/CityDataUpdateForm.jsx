import React from "react";
import { useState } from "react";
import "../design/index.css";

function CityDataUpdateForm({ data, handleUpdate, clearData }) {
  const [formData, setFormData] = useState({
    id: data.id,
    city: data.city,
    country: data.country,
    timeZone: data.timeZone,
    latitude: data.latitude,
    longitude: data.longitude,
  });

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleUpdateClick = () => {
    handleUpdate(data.id, formData);
    clearData();
  };

  return (
    <div>
      <h1>Update data</h1>
      <br />
      <label>Id: </label>
      <br />
      <input type="text" name="id" value={formData.id} disabled={true} />
      <br />
      <label>City name:</label>
      <br />
      <input type="text" name="city" value={formData.city} onChange={handleChange} />
      <br />
      <label>Country:</label>
      <br />
      <input type="text" name="country" value={formData.country} onChange={handleChange} />
      <br />
      <label>Timezone:</label>
      <br />
      <input type="text" name="timezone" value={formData.timeZone} onChange={handleChange} />
      <br />
      <label>Latitude:</label>
      <br />
      <input type="text" name="latitude" value={formData.latitude} onChange={handleChange} />
      <br />
      <label>Longitude:</label>
      <br />
      <input type="text" name="longitude" value={formData.longitude} onChange={handleChange} />
      <br />
      <button onClick={handleUpdateClick}>Update</button>
      <button onClick={clearData}>Cancel</button>
    </div>
  );
}

export default CityDataUpdateForm;
