const authenitcation = require("../controllers/authentication");
const metadata = require("../controllers/metadata");
const files = require("../controllers/files");
const tokens = require("../controllers/tokens");
const redis = require("../controllers/redis");
const conflict = require("../controllers/conflict");
const logger = require("../services/logger.js");
const drive = require("../controllers/drive");

module.exports = (app) => {
  app.get(
    "/api/files/:id",
    authenitcation.isAuthenticated,
    metadata.loadMetadata,
    metadata.checkPermissionsOnFile,
    metadata.checkSizeOfFile,
    conflict.resolver,
    tokens.generateAccessToken,
    files.generateUrl,
    (req, res) => {
      try {
        const id = req.params.id;
        const url = res.locals.url;
        const accessToken = res.locals.accessToken;
        const faviconUrl = res.locals.faviconUrl;
        const fileName = res.locals.metadata.name;
        res.render("index", {
          url: url,
          accessToken: accessToken,
          faviconUrl: faviconUrl,
          fileName: fileName,
        });
        logger.log({
          level: "info",
          message: "Index successfully rendered",
          label: `FileId: ${id}`,
        });
      } catch (e) {
        logger.log({
          level: "error",
          message: `Status 500, failed to render index, error: ${e}`,
          label: `FileId: ${req.params.id}`,
        });
        res.status(500).send(e);
      }
    }
  );

  app.post("/closeSession/:id", authenitcation.isAuthenticated, async (req, res) => {
    try {
      const id = req.params.id;
      const user = req.user;
      await redis.removeUserFromSession(id, user);
    } catch (e) {
      logger.log({
        level: "error",
        message: `Status 500, failed to remove user from session, error: ${e}`,
        label: `session: ${id} user: ${user}`,
      });
      res.status(500).send(e);
    }
  });

  app.get("/isalive", (req, res) => {
    return res.send("alive");
  });

  app.get("/updateAndDownload/:id",
    authenitcation.isAuthenticated,
    metadata.loadMetadata,
    metadata.checkPermissionsOnFile,
    files.updateFile,
    drive.redirectToDriveDownload,
  );
};
