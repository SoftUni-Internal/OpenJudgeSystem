/* eslint-disable import/exports-last */
/* eslint-disable import/no-unused-modules */
import InfoIcon from '@mui/icons-material/Info';
import { Box, List, ListItem, Tooltip } from '@mui/material';

import styles from './LegendBox.module.scss';

interface ILegendBoxProps {
    renders: Array<{color: string; message:string}>;
}

const LegendBox = (props: ILegendBoxProps) => {
    const { renders } = props;
    return (
        <Box className={styles.legendBox}>
            <Tooltip
              color="info"
              title={(
                  <List sx={{ p: 0 }}>
                      {renders.map((x) => (
                          <ListItem key={x.message}>
                              <Box className={styles.legendBox}>
                                  <Box className={styles.rowColorBox}>
                                      <Box
                                        sx={{ backgroundColor: `${x.color}` }}
                                        className={`${styles.colorBox}`}
                                      />
                                      <p className={styles.colorSeparator}>-</p>
                                      <p>{x.message}</p>
                                  </Box>
                              </Box>
                          </ListItem>
                      ))}
                  </List>
)}
            >
                <InfoIcon sx={{ width: '35px', height: '35px', marginRight: '3rem' }} />
            </Tooltip>
        </Box>
    );
};

export default LegendBox;
