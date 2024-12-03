# How Angular and Auth0 Work Together

## User Login

1. The user logs in through your Angular app, using:
   - Auth0’s hosted login page
   - An embedded login widget
2. Auth0 authenticates the user and provides:
   - An ID token (user identity)
   - An access token (JWT for API)

## Access Secured API

Angular uses the access token in its HTTP requests by:

1. Adding `Authorization: Bearer <token>` to the headers.
2. Sending requests to your secured API (`http://localhost:5016/api/Product`).

## Backend Validation

Your API validates the token by:

1. Checking the signature to ensure it’s issued by Auth0.
2. Verifying the token hasn’t expired.
3. Ensuring the audience (aud) matches your API.
4. Optionally, checking claims like roles or permissions.
