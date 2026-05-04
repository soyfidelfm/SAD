'use strict';

customElements.define('compodoc-menu', class extends HTMLElement {
    constructor() {
        super();
        this.isNormalMode = this.getAttribute('mode') === 'normal';
    }

    connectedCallback() {
        this.render(this.isNormalMode);
    }

    render(isNormalMode) {
        let tp = lithtml.html(`
        <nav>
            <ul class="list">
                <li class="title">
                    <a href="index.html" data-type="index-link">sadweb-app documentation</a>
                </li>

                <li class="divider"></li>
                ${ isNormalMode ? `<div id="book-search-input" role="search"><input type="text" placeholder="Type to search"></div>` : '' }
                <li class="chapter">
                    <a data-type="chapter-link" href="index.html"><span class="icon ion-ios-home"></span>Getting started</a>
                    <ul class="links">
                                <li class="link">
                                    <a href="overview.html" data-type="chapter-link">
                                        <span class="icon ion-ios-keypad"></span>Overview
                                    </a>
                                </li>

                            <li class="link">
                                <a href="index.html" data-type="chapter-link">
                                    <span class="icon ion-ios-paper"></span>
                                        README
                                </a>
                            </li>
                                <li class="link">
                                    <a href="dependencies.html" data-type="chapter-link">
                                        <span class="icon ion-ios-list"></span>Dependencies
                                    </a>
                                </li>
                                <li class="link">
                                    <a href="properties.html" data-type="chapter-link">
                                        <span class="icon ion-ios-apps"></span>Properties
                                    </a>
                                </li>

                    </ul>
                </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#components-links"' :
                            'data-bs-target="#xs-components-links"' }>
                            <span class="icon ion-md-cog"></span>
                            <span>Components</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="components-links"' : 'id="xs-components-links"' }>
                            <li class="link">
                                <a href="components/AddPopupComponent.html" data-type="entity-link" >AddPopupComponent</a>
                            </li>
                            <li class="link">
                                <a href="components/App.html" data-type="entity-link" >App</a>
                            </li>
                            <li class="link">
                                <a href="components/AuthCallbackComponent.html" data-type="entity-link" >AuthCallbackComponent</a>
                            </li>
                            <li class="link">
                                <a href="components/DashboardComponent.html" data-type="entity-link" >DashboardComponent</a>
                            </li>
                            <li class="link">
                                <a href="components/LoginComponent.html" data-type="entity-link" >LoginComponent</a>
                            </li>
                            <li class="link">
                                <a href="components/SalesComponent.html" data-type="entity-link" >SalesComponent</a>
                            </li>
                            <li class="link">
                                <a href="components/SettingsComponent.html" data-type="entity-link" >SettingsComponent</a>
                            </li>
                            <li class="link">
                                <a href="components/ShellComponent.html" data-type="entity-link" >ShellComponent</a>
                            </li>
                            <li class="link">
                                <a href="components/StoresComponent.html" data-type="entity-link" >StoresComponent</a>
                            </li>
                        </ul>
                    </li>
                        <li class="chapter">
                            <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#injectables-links"' :
                                'data-bs-target="#xs-injectables-links"' }>
                                <span class="icon ion-md-arrow-round-down"></span>
                                <span>Injectables</span>
                                <span class="icon ion-ios-arrow-down"></span>
                            </div>
                            <ul class="links collapse " ${ isNormalMode ? 'id="injectables-links"' : 'id="xs-injectables-links"' }>
                                <li class="link">
                                    <a href="injectables/AuthService.html" data-type="entity-link" >AuthService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/CatalogService.html" data-type="entity-link" >CatalogService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/CreditCardApplicationsService.html" data-type="entity-link" >CreditCardApplicationsService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/DashboardService.html" data-type="entity-link" >DashboardService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/MembershipSalesService.html" data-type="entity-link" >MembershipSalesService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/SalesService.html" data-type="entity-link" >SalesService</a>
                                </li>
                                <li class="link">
                                    <a href="injectables/UserDailySettingsService.html" data-type="entity-link" >UserDailySettingsService</a>
                                </li>
                            </ul>
                        </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#interceptors-links"' :
                            'data-bs-target="#xs-interceptors-links"' }>
                            <span class="icon ion-ios-swap"></span>
                            <span>Interceptors</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="interceptors-links"' : 'id="xs-interceptors-links"' }>
                            <li class="link">
                                <a href="interceptors/AuthInterceptor.html" data-type="entity-link" >AuthInterceptor</a>
                            </li>
                        </ul>
                    </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#interfaces-links"' :
                            'data-bs-target="#xs-interfaces-links"' }>
                            <span class="icon ion-md-information-circle-outline"></span>
                            <span>Interfaces</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? ' id="interfaces-links"' : 'id="xs-interfaces-links"' }>
                            <li class="link">
                                <a href="interfaces/AuthResponse.html" data-type="entity-link" >AuthResponse</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CatalogMembership.html" data-type="entity-link" >CatalogMembership</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CatalogStore.html" data-type="entity-link" >CatalogStore</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CreateCreditCardApplicationDto.html" data-type="entity-link" >CreateCreditCardApplicationDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CreatedMembershipSaleResponse.html" data-type="entity-link" >CreatedMembershipSaleResponse</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CreatedResponse.html" data-type="entity-link" >CreatedResponse</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CreateMembershipSaleDto.html" data-type="entity-link" >CreateMembershipSaleDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CreateUserDailySetting.html" data-type="entity-link" >CreateUserDailySetting</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CreditCardApplicationDto.html" data-type="entity-link" >CreditCardApplicationDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/CreditCardApplicationsSummaryDto.html" data-type="entity-link" >CreditCardApplicationsSummaryDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/DashboardSummaryDto.html" data-type="entity-link" >DashboardSummaryDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/ExternalLoginRequest.html" data-type="entity-link" >ExternalLoginRequest</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/LatestTransactionDto.html" data-type="entity-link" >LatestTransactionDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/MembershipSaleDto.html" data-type="entity-link" >MembershipSaleDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/MembershipSalesSummaryDto.html" data-type="entity-link" >MembershipSalesSummaryDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/MeResponse.html" data-type="entity-link" >MeResponse</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/NavItem.html" data-type="entity-link" >NavItem</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/Sale.html" data-type="entity-link" >Sale</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/SaleCreateDto.html" data-type="entity-link" >SaleCreateDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/SalesByHour.html" data-type="entity-link" >SalesByHour</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/TodaySalesSummaryDto.html" data-type="entity-link" >TodaySalesSummaryDto</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/UpdateUserDailySetting.html" data-type="entity-link" >UpdateUserDailySetting</a>
                            </li>
                            <li class="link">
                                <a href="interfaces/UserDailySetting.html" data-type="entity-link" >UserDailySetting</a>
                            </li>
                        </ul>
                    </li>
                    <li class="chapter">
                        <div class="simple menu-toggler" data-bs-toggle="collapse" ${ isNormalMode ? 'data-bs-target="#miscellaneous-links"'
                            : 'data-bs-target="#xs-miscellaneous-links"' }>
                            <span class="icon ion-ios-cube"></span>
                            <span>Miscellaneous</span>
                            <span class="icon ion-ios-arrow-down"></span>
                        </div>
                        <ul class="links collapse " ${ isNormalMode ? 'id="miscellaneous-links"' : 'id="xs-miscellaneous-links"' }>
                            <li class="link">
                                <a href="miscellaneous/functions.html" data-type="entity-link">Functions</a>
                            </li>
                            <li class="link">
                                <a href="miscellaneous/typealiases.html" data-type="entity-link">Type aliases</a>
                            </li>
                            <li class="link">
                                <a href="miscellaneous/variables.html" data-type="entity-link">Variables</a>
                            </li>
                        </ul>
                    </li>
                        <li class="chapter">
                            <a data-type="chapter-link" href="routes.html"><span class="icon ion-ios-git-branch"></span>Routes</a>
                        </li>
                    <li class="chapter">
                        <a data-type="chapter-link" href="coverage.html"><span class="icon ion-ios-stats"></span>Documentation coverage</a>
                    </li>
                    <li class="divider"></li>
                    <li class="copyright">
                        Documentation generated using <a href="https://compodoc.app/" target="_blank" rel="noopener noreferrer">
                            <img data-src="images/compodoc-vectorise.png" class="img-responsive" data-type="compodoc-logo">
                        </a>
                    </li>
            </ul>
        </nav>
        `);
        this.innerHTML = tp.strings;
    }
});