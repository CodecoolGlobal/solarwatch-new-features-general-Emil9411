import React from 'react';
import { useNavigate } from "react-router-dom";
import "../design/index.css";

function Registration(){
    const navigate = useNavigate()

    async function handleSubmit(event) {
        event.preventDefault();
        const userName = event.target.username.value;
        const email = event.target.email.value;
        const password = event.target.password.value;
        const city = event.target.city.value;
        const user = {userName, email, password, city};
        try{
            const response = await fetch("/api/Auth/register",{
                method: "POST",
                headers:{
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(user)
            })
            console.log(user)
            alert("Registration successful!");
            navigate("/login");
        } catch(e) {
            alert("Registration failed!");
            console.log(e);
        }
    }

    return (
        <div className="registration">
            <form className="registrationForm" onSubmit={handleSubmit} >
                <label>Username:</label>
                <br></br>
                <input type="text" name="username"></input>
                <br></br>
                <label>Email:</label>
                <br></br>
                <input type="email" name="email"></input>
                <br></br>
                <label>Password:</label>
                <br></br>
                <input type="password" name="password"></input>
                <br></br>
                <label>City:</label>
                <br></br>
                <input type="text"name="city"></input>
                <br></br>
                <button type="submit">Register</button>
                <button type="button" onClick={() => navigate("/")}>Cancel</button>
            </form>
        </div>
    )
};

export default Registration;