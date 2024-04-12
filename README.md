# SolarWatch Documentation

## Table of Contents
  - [Introduction](#introduction)
  - [Usage](#usage)
  - [API](#api)

## Introduction
SolarWatch is a web application that allows users to check a city's sunrise and sunset times, based on the given date.

## Usage
To use SolarWatch, simply enter the city name and the date you want to check the sunrise and sunset times for. The application will then display the sunrise and sunset times for that city on the given date, in local time. This functionality is available to logged in users only. They can register with a city name, which will be saved in the database, and can be used to check the sunrise and sunset times for that city without having to enter it every time. The admin user can view all the registered cities with their coordinates, country, and timezone, and all the already asked "solar watch" data, which includes the city name, date, sunrise, and sunset times. The admin user can also delete, or update the city information, and the "solar watch" data. The admin user can also view all the users. And lastly both admin and regular users can use delete and update endpoints to delete or update their own data, the admin user can delete or update any data.

## API
The SolarWatch API provides the following endpoints:

### Users
- `api/user/getAll` - GET request to get all users. Only available to admin user.
- `api/user/getbyuserdata/{emailorusername}` - GET request to get a user by email or username. Available to both admin and regular users.
- `api/user/getbyid/{id}` - GET request to get a user by id. Only available to admin user.
- `api/user/delete/{id}` - DELETE request to delete a user by id. Available to both admin and regular users.
- `api/user/update/{id}` - PATCH request to update a user by id. Available to both admin and regular users.

### Auth
- `api/auth/login` - POST request to login. Available to anyone.
- `api/auth/register` - POST request to register. Available to anyone. Admin user can only be created by the application owner.
- `api/auth/logout` - POST request to logout. Available to both admin and regular users.
- `api/auth/whoami` - GET request to get the current user. Available to both admin and regular users.

### Location
- `api/location/getlocation/{city}` - GET request to get the coordinates, country, and timezone of a city. Available to both admin and regular users.
- `api/location/getAll` - GET request to get all locations. Only available to admin users.
- `api/location/delete/{id}` - DELETE request to delete a location by id. Only available to admin user.
- `api/location/update/{id}` - PATCH request to update a location by id. Only available to admin user.

### SolarWatch
- `api/sw/getdata/{city}/{date}` - GET request to get the sunrise and sunset times for a city on a given date. Available to both admin and regular users.
- `api/sw/getAll` - GET request to get all the "solar watch" data. Only available to admin user.
- `api/sw/delete/{id}` - DELETE request to delete "solar watch" data by id. Only available to admin user.
- `api/sw/update/{id}` - PATCH request to update "solar watch" data by id. Only available to admin user. 
