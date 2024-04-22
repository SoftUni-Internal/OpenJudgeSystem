/* eslint-disable @typescript-eslint/ban-types */
import { IconType } from 'react-icons/lib/cjs/iconBase';
import { RiAddBoxFill } from 'react-icons/ri';
import { IconButton, Tooltip } from '@mui/material';

import { CREATE_NEW_RECORD } from '../../../../common/labels';
import { ACTION_NOT_ALLOWED_MESSAGE } from '../../../../common/messages';
import { getColors } from '../../../../hooks/use-administration-theme-provider';
import { useAppSelector } from '../../../../redux/store';

interface ICreateButtonProps {
    showModal: boolean;
    showModalFunc: Function;
    styles: any;
    Icon? : IconType;
    tooltipLabel?: string;
    disabled? :boolean;
    disabledMessage?: string;
}
const CreateButton = (props: ICreateButtonProps) => {
    const {
        showModal,
        showModalFunc,
        styles,
        Icon,
        tooltipLabel = CREATE_NEW_RECORD,
        disabled = false,
        disabledMessage = ACTION_NOT_ALLOWED_MESSAGE,
    } = props;

    const themeMode = useAppSelector((x) => x.theme.administrationMode);
    return (
        <Tooltip title={disabled
            ? disabledMessage
            : tooltipLabel}
        >
            <span>
                <IconButton
                  disabled={disabled}
                  onClick={() => showModalFunc(!showModal)}
                >

                    {Icon
                        ? <Icon style={styles} />
                        : (
                            <RiAddBoxFill
                              style={{
                                  ...styles,
                                  color: Object.keys(styles).find((x) => x === 'color')
                                      ? styles.color
                                      : getColors(themeMode).palette.primary.main,
                              }}
                            />
                        )}
                </IconButton>
            </span>
        </Tooltip>
    );
};
export default CreateButton;
