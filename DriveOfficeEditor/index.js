const express = require("express");
const cookieParser = require("cookie-parser");
const session = require("express-session");
const editor = require("./routes/editor");
const auth = require("./routes/authentication");
const localOffice = require("./routes/localOffice");
const newPage = require("./routes/newPage");
const logger = require("./services/logger.js");

const app = express();
app.use(cookieParser());
app.set("view engine", "ejs");
app.use(
  session({
    secret: "passport",
    cookie: { maxAge: 7 * 24 * 60 * 60000 },
    resave: false,
    saveUninitialized: true,
  })
);

// adding routes for authentication
auth(app);

// adding routes for creating blank pages
newPage(app);

// adding the main route of editing/viewing files
editor(app);

// adding the route of editing/viewing files in local office
localOffice(app);

app.listen(process.env.PORT, () =>
  logger.log({
    level: "info",
    message: `Drive Office Editor is listening on http://localhost:${process.env.PORT}`,
    label: "DriveOfficeEditor up",
  })
);
