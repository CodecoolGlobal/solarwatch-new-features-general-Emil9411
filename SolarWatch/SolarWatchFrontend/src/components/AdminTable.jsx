import React, { useState, useEffect } from "react";
import { capitalizeWords } from "../utilities/utils";
import MessageModal from "./MessageModal";
import "../design/table.css";

const AdminTable = ({ data, handleDelete, handleUpdate }) => {
  const [keys, setKeys] = useState([]);
  const [showModal, setShowModal] = useState(false);
  const [deleteItemId, setDeleteItemId] = useState(null);

  useEffect(() => {
    if (data.length > 0) {
      setKeys(Object.keys(data[0]));
    }
  }, [data]);

  function handleDeleteClick(id) {
    setDeleteItemId(id);
    setShowModal(true);
  }

  function handleConfirmDelete() {
    handleDelete(deleteItemId);
    setShowModal(false);
    setDeleteItemId(null);
  }

  function handleCloseModal() {
    setShowModal(false);
    setDeleteItemId(null);
  }

  return (
    <>
      <MessageModal
        isOpen={showModal}
        message="Are you sure you want to delete this item?"
        handleClose={handleCloseModal}
        handleConfirm={handleConfirmDelete}
      />
      {data.length === 0 ? (
        <h1>No data to display</h1>
      ) : (
        <table>
          <thead>
            <tr>
              {keys.map((key) => (
                <th key={key}>{capitalizeWords(key)}</th>
              ))}
              <th>Update</th>
              <th>Delete</th>
            </tr>
          </thead>
          <tbody>
            {data.map((row, index) => (
              <tr key={index}>
                {keys.map((key) => (
                  <td key={key}>{row[key]}</td>
                ))}
                <td>
                  <button onClick={() => handleUpdate(row)}>Update</button>
                </td>
                <td>
                  <button onClick={() => handleDeleteClick(row.id)}>Delete</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  );
};

export default AdminTable;
