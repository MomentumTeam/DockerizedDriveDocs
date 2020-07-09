const metadataService = require("../services/metadataService");
const permissions = {
  READONLY: "readonly",
  WRITE: "write",
};
const operations = {
  VIEW: "view",
  EDIT: "edit",
  EDIT_NEW: "editNew",
};

exports.loadMetadata = async (req, res, next) => {
  try {
    if (req.query.operation == operations.EDIT_NEW) {
      next();
    } else {
      try {
        const fileId = req.query.template ? req.query.template : req.params.id;
        let metadata = await metadataService.getMetadata(fileId, req.user.id);
        metadata.type = metadata.name.substring(metadata.name.indexOf(".") + 1, metadata.name.length);
        res.locals.metadata = metadata;
        if (res.locals.metadata.hasOwnProperty("permission")) {
          delete res.locals.metadata["permission"];
        }

        next();
      } catch (error) {
        return res.status(500).send("File does not exist");
      }
    }
  } catch (e) {
    return res.status(500).send("File does not exist");
  }
};

exports.checkPermissionsOnFile = (req, res, next) => {
  try {
    if (req.query.operation == operations.EDIT_NEW) {
      req.user.permission = permissions.WRITE;
      next();
    } else {
      const metadata = res.locals.metadata;
      if (req.query.operation == operations.EDIT && metadata.role != "OWNER" && metadata.role != "WRITE") {
        return res.status(404).send("You do not have the right permission!");
      } else if (req.query.operation == operations.VIEW && metadata.role != "OWNER" && metadata.role != "WRITE" && metadata.role != "READ") {
        return res.status(404).send("You do not have the right permission!");
      } else {
        next();
      }
    }
  } catch (e) {
    return res.status(404).send("You do not have the right permission!");
  }
};
