import React from "react";
import { useState } from "react";
import "../design/index.css";

function AdminUpdateForm({ data, onUpdate, clearData }) {
  const [formData, setFormData] = useState({
    id: data.id,
    city: data.city,
    date: data.date,
    sunrise: data.sunrise,
    sunset: data.sunset,
  });

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleUpdateClick = () => {
    onUpdate(data.id, formData);
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
      <label>Date:</label>
      <br />
      <input type="date" name="date" value={formData.date} onChange={handleChange} />
      <br />
      <label>Sunrise:</label>
      <br />
      <input type="text" name="sunrise" value={formData.sunrise} onChange={handleChange} />
      <br />
      <label>Sunset:</label>
      <br />
      <input type="text" name="sunset" value={formData.sunset} onChange={handleChange} />
      <br />
      <button onClick={handleUpdateClick}>Update</button>
    </div>
  );
}

export default AdminUpdateForm;
