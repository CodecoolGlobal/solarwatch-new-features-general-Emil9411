import React, { useState } from "react";
import "../../design/index.css";

function UserDataUpdateForm({ data, handleUpdate, clearData }) {
  const [formData, setFormData] = useState({
    id: data.id,
    email: data.email,
    userName: data.userName,
    city: data.city,
    phoneNumber: data.phoneNumber,
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
      <label>Email:</label>
      <br />
      <input type="text" name="email" value={formData.email} onChange={handleChange} />
      <br />
      <label>Username:</label>
      <br />
      <input type="text" name="userName" value={formData.userName} onChange={handleChange} />
      <br />
      <label>City:</label>
      <br />
      <input type="text" name="city" value={formData.city} onChange={handleChange} />
      <br />
      <label>Phone Number:</label>
      <br />
      <input
        type="text"
        name="phoneNumber"
        value={formData.phoneNumber}
        onChange={handleChange}
      />
      <br />
      <button onClick={handleUpdateClick}>Update</button>
      <button onClick={clearData}>Cancel</button>
    </div>
  );
}

export default UserDataUpdateForm;
