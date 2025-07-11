import React, { ReactNode } from 'react';
import { Box, Slide, Tab, Tabs } from '@mui/material';

interface IAdministrationTabModel {
    value: string;
    label:string;
    node:(value: string) => ReactNode;
    disabled?: boolean;
}

interface ITabsInViewProps {
    form?: () => ReactNode;
    tabs? : Array<IAdministrationTabModel>;
    tabName?: string;
    onTabChange?: (event: React.SyntheticEvent, newValue: any) => void;
}

const TabsInView = (props: ITabsInViewProps) => {
    const { form, tabs = [], tabName, onTabChange } = props;

    return (
        <Slide direction="left" in mountOnEnter unmountOnExit timeout={300}>
            <Box>
                {form?.()}
                <Box sx={{ padding: '2rem' }}>
                    <Tabs
                      sx={{
                          minWidth: '100%',
                          display: 'flex',
                          justifyContent: 'space-around',
                          '& .MuiTabs-flexContainer': { flexWrap: 'wrap' },
                      }}
                      value={tabName}
                      onChange={onTabChange}
                      aria-label="wrapped label tabs example"
                      variant="scrollable"
                      scrollButtons="auto"
                    >
                        {tabs.map((x: IAdministrationTabModel) => 
                            <Tab
                              key={x.value}
                              sx={{
                                  minWidth: 'auto',
                                  flex: '1 1 auto',
                                  maxWidth: 'none',
                                  display: 'flex',
                                  justifyContent: 'center',
                              }}
                              value={x.value}
                              label={x.label}
                              disabled={!!x.disabled}
                              wrapped
                            />)}
                    </Tabs>
                </Box>
                {tabs.map((x: IAdministrationTabModel) => tabName === x.value && 
                <React.Fragment key={x.value}>
                    {x.node(x.value)}
                </React.Fragment>)}
            </Box>
        </Slide>
    );
};

export default TabsInView;
