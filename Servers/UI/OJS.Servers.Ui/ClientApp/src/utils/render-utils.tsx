/* eslint-disable react/jsx-no-useless-fragment */
import { Alert, AlertHorizontalOrientation, AlertSeverity, AlertVariant, AlertVerticalOrientation } from '../components/guidelines/alert/Alert';

const renderAlert = (
    message: string,
    severity: AlertSeverity,
    index: number,
    autoHideDuration?: number | undefined,
) => (
    <Alert
      key={index}
      autoHideDuration={autoHideDuration}
      variant={AlertVariant.Filled}
      vertical={AlertVerticalOrientation.Top}
      horizontal={AlertHorizontalOrientation.Right}
      severity={severity}
      message={message}
      styles={{ marginTop: `${index * 4}rem` }}
    />
);

const renderSuccessfullAlert = (message: string | null, autoHideDuration?: number) => {
    if (!message) {
        return (
            <></>
        );
    }

    if (autoHideDuration) {
        return renderAlert(message, AlertSeverity.Success, 0, autoHideDuration);
    }

    return renderAlert(message, AlertSeverity.Success, 0, 3000);
};

const renderErrorMessagesAlert = (
    messages:Array<string>,
    autoHideDuration?: number,
) => messages.map((x, i) => renderAlert(
    x,
    AlertSeverity.Error,
    i,
    autoHideDuration,
));

const renderInfoMessage = (
    message: string,
    autoHideDuration?: number,
) => renderAlert(
    message,
    AlertSeverity.Info,
    0,
    autoHideDuration,
);

export {
    renderSuccessfullAlert,
    renderErrorMessagesAlert,
    renderInfoMessage,
};
