const metadataService = require("../services/metadataService");
const logger = require("../services/logger.js");
const { config } = require("../config/config");

const PDF = config.fileTypes.PDF;
const operations = config.operations;
const roles = config.roles;
const maxSizes = config.maxSizes;
const permissions = config.permissions;

exports.loadMetadata = async (req, res, next) => {
  try {
    try {
      const fileId = req.params.id;
      let metadata = await metadataService.getMetadata(fileId, req.user);
      metadata.type = metadata.name.substring(metadata.name.lastIndexOf(".") + 1, metadata.name.length).toLowerCase();
      res.locals.metadata = metadata;
      if (res.locals.metadata.hasOwnProperty("permission")) {
        delete res.locals.metadata["permission"];
      }
      logger.log({
        level: "info",
        message: "Received Metadata of file successfuly",
        label: `fileId ${req.params.id}`,
      });
      next();
    } catch (error) {
      logger.log({
        level: "error",
        message: "Status 500: Error in receiving the metadata: " + error,
        label: `session: ${req.params.id}`,
      });
      return res.status(500).send("Error in receiving the metadata, or file may not exist");
    }
  } catch (e) {
    logger.log({
      level: "error",
      message: "status 500: Error in receiving the metadata",
      label: `session: ${req.params.id}`,
    });
    return res.status(500).send("Error in receiving the metadata, or file may not exist");
  }
};

exports.checkPermissionsOnFile = (req, res, next) => {
  try {
    const metadata = res.locals.metadata;
    res.locals.permission = permissions.WRITE;
    if (metadata.role == roles.OWNER || metadata.role == roles.WRITE) {
      req.query.operation = req.query.operation ? req.query.operation : operations.EDIT;
    } else if (metadata.role == roles.READ) {
      res.locals.permission = permissions.READ;
      req.query.operation = operations.VIEW;
    } else {
      logger.log({
        level: "error",
        message: "Status 403: Permission denied",
        label: `user: ${req.user.id}`,
      });
      return res.status(403).send("You do not have the right permission!");
    }
    logger.log({
      level: "info",
      message: "Permission granted",
      label: `user: ${req.user.id}`,
    });
    next();
  } catch (e) {
    return res.status(403).send("You do not have the right permission!");
  }
};

exports.setViewPermissionsOnFile = (req, res, next) => {
      res.locals.permission = permissions.READ;
      req.query.operation = operations.VIEW;
    next();
};

exports.checkSizeOfFile = (req, res, next) => {
  try {
    const metadata = res.locals.metadata;
    const maxSize = maxSizes[metadata.type] != undefined ? maxSizes[metadata.type] : maxSizes[PDF];
    if (metadata.size > maxSize) {
      logger.log({
        level: "error",
        message: `Status 413: The file is too big since its size is ${metadata.size}`,
        label: `file: ${req.params.id}`,
      });
      return res.status(413).send(`The file is too big since its size is ${metadata.size}`);
    }
    logger.log({
      level: "info",
      message: `Size is ok`,
      label: `file: ${req.params.id}`,
    });
    next();
  } catch (e) {
    logger.log({
      level: "error",
      message: `Status 500: Error in determining the file size: ${e}`,
      label: `file: ${req.params.id}`,
    });
    return res.status(500).send("the file is too big");
  }
};
