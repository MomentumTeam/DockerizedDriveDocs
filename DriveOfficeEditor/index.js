const express = require("express");
const cookieParser = require("cookie-parser");
const bodyParser = require("body-parser");
const session = require("express-session");
const editor = require("./routes/editor");
const auth = require("./routes/authentication");
const localOffice = require("./routes/localOffice");
const newPage = require("./routes/newPage");
const app = express();
// require("dotenv").config();
const logger = require("./services/logger.js");
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
auth(app);
newPage(app);
editor(app);
localOffice(app);

app.listen(process.env.PORT, () =>
  logger.log({
    level: "info",
    message: `Drive Office Editor is listening at http://localhost:${process.env.PORT}`,
    label: "DriveOfficeEditor up",
  })
);
