import React from "react";
import "../design/table.css";
import { capitalizeWords } from "../utilities/utils";

function OneRowTable({ dataObject }) {
    const filteredKeys = Object.keys(dataObject).filter(key => key !== "id");
    return (
        <table>
        <tbody>
            {filteredKeys.map((key) => (
            <tr key={key}>
                <td>{capitalizeWords(key)}</td>
                <td>{dataObject[key]}</td>
            </tr>
            ))}
            <tr>
                <td><button>Save City</button></td>
                <td><button>Save City & Date</button></td>
            </tr>
        </tbody>
        </table>
    );
}

export default OneRowTable;