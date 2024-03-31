import React from "react";
import "../design/oneRowTable.css";
import { capitalizeWords } from "../utilities/utils";

const AdminTable = ({ data }) => {
  // Extract the keys from the first row of data
  const keys = Object.keys(data[0]);

  return (
    <table>
      <thead>
        <tr>
          {keys.map((key) => (
            <th key={key}>{capitalizeWords(key)}</th>
          ))}
          <th>Edit</th>
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
              <button>Edit</button>
            </td>
            <td>
              <button>Delete</button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};

export default AdminTable;
