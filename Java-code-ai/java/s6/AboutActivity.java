package java.s6;

import static com.amaze.filemanager.utils.Utils.openURL;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.MenuItem;
import android.view.View;
import android.widget.TextView;

import androidx.annotation.Nullable;
import androidx.appcompat.widget.Toolbar;
import androidx.coordinatorlayout.widget.CoordinatorLayout;
import androidx.palette.graphics.Palette;

import com.amaze.filemanager.LogHelper;
import com.amaze.filemanager.R;
import com.amaze.filemanager.ui.activities.superclasses.BasicActivity;
import com.amaze.filemanager.ui.theme.AppTheme;
import com.amaze.filemanager.utils.Billing;
import com.amaze.filemanager.utils.Utils;
import com.google.android.material.appbar.AppBarLayout;
import com.google.android.material.appbar.CollapsingToolbarLayout;
import com.mikepenz.aboutlibraries.Libs;
import com.mikepenz.aboutlibraries.LibsBuilder;

public class AboutActivity extends BasicActivity implements View.OnClickListener {

    private static final String TAG = "AboutActivity";
    private static final int HEADER_HEIGHT = 1024;
    private static final int HEADER_WIDTH = 500;

    private static final String URL_AUTHOR1_GITHUB = "https://github.com/arpitkh96";
    private static final String URL_AUTHOR2_GITHUB = "https://github.com/VishalNehra";
    private static final String URL_DEVELOPER1_GITHUB = "https://github.com/EmmanuelMess";
    private static final String URL_DEVELOPER2_GITHUB = "https://github.com/TranceLove";
    private static final String URL_REPO_CHANGELOG = "https://github.com/TeamAmaze/AmazeFileManager/commits/master";
    private static final String URL_REPO = "https://github.com/TeamAmaze/AmazeFileManager";
    private static final String URL_REPO_ISSUES = "https://github.com/TeamAmaze/AmazeFileManager/issues";
    private static final String URL_REPO_TRANSLATE = "https://www.transifex.com/amaze/amaze-file-manager/";
    private static final String URL_REPO_XDA = "http://forum.xda-developers.com/android/apps-games/app-amaze-file-managermaterial-theme-t2937314";
    private static final String URL_REPO_RATE = "market://details?id=com.amaze.filemanager";

    private AppBarLayout mAppBarLayout;
    private CollapsingToolbarLayout mCollapsingToolbarLayout;
    private TextView mTitleTextView;
    private View mAuthorsDivider, mDeveloper1Divider;
    private Billing billing;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        applyTheme();
        setContentView(R.layout.activity_about);
        initViews();
        setupToolbar();
        applyHeaderImageColors();
        setupScrollEffects();
    }

    private void applyTheme() {
        switch (getAppTheme()) {
            case DARK:
                setTheme(R.style.aboutDark);
                break;
            case BLACK:
                setTheme(R.style.aboutBlack);
                break;
            default:
                setTheme(R.style.aboutLight);
        }
    }

    private void initViews() {
        mAppBarLayout = findViewById(R.id.appBarLayout);
        mCollapsingToolbarLayout = findViewById(R.id.collapsing_toolbar_layout);
        mTitleTextView = findViewById(R.id.text_view_title);
        mAuthorsDivider = findViewById(R.id.view_divider_authors);
        mDeveloper1Divider = findViewById(R.id.view_divider_developers_1);
        mAppBarLayout.setLayoutParams(calculateHeaderViewParams());
        switchIcons();
    }

    private void setupToolbar() {
        Toolbar mToolbar = findViewById(R.id.toolBar);
        setSupportActionBar(mToolbar);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        getSupportActionBar().setHomeAsUpIndicator(getResources().getDrawable(R.drawable.md_nav_back));
        getSupportActionBar().setDisplayShowTitleEnabled(false);
    }

    private void applyHeaderImageColors() {
        Bitmap bitmap = BitmapFactory.decodeResource(getResources(), R.drawable.about_header);
        Palette.from(bitmap).generate(palette -> {
            int mutedColor = palette.getMutedColor(Utils.getColor(this, R.color.primary_blue));
            int darkMutedColor = palette.getDarkMutedColor(Utils.getColor(this, R.color.primary_blue));
            mCollapsingToolbarLayout.setContentScrimColor(mutedColor);
            mCollapsingToolbarLayout.setStatusBarScrimColor(darkMutedColor);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                getWindow().setStatusBarColor(darkMutedColor);
            }
        });
    }

    private void setupScrollEffects() {
        mAppBarLayout.addOnOffsetChangedListener((appBarLayout, verticalOffset) ->
            mTitleTextView.setAlpha(Math.abs(verticalOffset / (float) appBarLayout.getTotalScrollRange())));

        mAppBarLayout.setOnFocusChangeListener((v, hasFocus) ->
            mAppBarLayout.setExpanded(hasFocus, true));
    }

    private CoordinatorLayout.LayoutParams calculateHeaderViewParams() {
        CoordinatorLayout.LayoutParams layoutParams =
            (CoordinatorLayout.LayoutParams) mAppBarLayout.getLayoutParams();
        float aspectRatio = (float) HEADER_WIDTH / HEADER_HEIGHT;
        int screenWidth = getResources().getDisplayMetrics().widthPixels;
        layoutParams.width = screenWidth;
        layoutParams.height = (int) (screenWidth * aspectRatio);
        return layoutParams;
    }

    private void switchIcons() {
        if (getAppTheme().equals(AppTheme.DARK) || getAppTheme().equals(AppTheme.BLACK)) {
            int color = Utils.getColor(this, R.color.divider_dark_card);
            mAuthorsDivider.setBackgroundColor(color);
            mDeveloper1Divider.setBackgroundColor(color);
        }
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        if (item.getItemId() == android.R.id.home) {
            onBackPressed();
            return true;
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.relative_layout_source:
                openURL(URL_REPO, this);
                break;
            case R.id.relative_layout_issues:
                openURL(URL_REPO_ISSUES, this);
                break;
            case R.id.relative_layout_changelog:
                openURL(URL_REPO_CHANGELOG, this);
                break;
            case R.id.relative_layout_licenses:
                openLicenses();
                break;
            case R.id.text_view_author_1_github:
                openURL(URL_AUTHOR1_GITHUB, this);
                break;
            case R.id.text_view_author_2_github:
                openURL(URL_AUTHOR2_GITHUB, this);
                break;
            case R.id.text_view_developer_1_github:
                openURL(URL_DEVELOPER1_GITHUB, this);
                break;
            case R.id.text_view_developer_2_github:
                openURL(URL_DEVELOPER2_GITHUB, this);
                break;
            case R.id.relative_layout_translate:
                openURL(URL_REPO_TRANSLATE, this);
                break;
            case R.id.relative_layout_xda:
                openURL(URL_REPO_XDA, this);
                break;
            case R.id.relative_layout_rate:
                openURL(URL_REPO_RATE, this);
                break;
            case R.id.relative_layout_donate:
                billing = new Billing(this);
                break;
        }
    }

    private void openLicenses() {
        LibsBuilder builder = new LibsBuilder()
            .withLibraries("apachemina")
            .withActivityTitle(getString(R.string.libraries))
            .withAboutIconShown(true)
            .withAboutVersionShownName(true)
            .withAboutVersionShownCode(false)
            .withAboutDescription(getString(R.string.about_amaze))
            .withAboutSpecial1(getString(R.string.license))
            .withAboutSpecial1Description(getString(R.string.amaze_license))
            .withLicenseShown(true);

        switch (getAppTheme().getSimpleTheme()) {
            case LIGHT:
                builder.withActivityStyle(Libs.ActivityStyle.LIGHT_DARK_TOOLBAR);
                break;
            case DARK:
                builder.withActivityStyle(Libs.ActivityStyle.DARK);
                break;
            case BLACK:
                builder.withActivityTheme(R.style.AboutLibrariesTheme_Black);
                break;
            default:
                LogHelper.logOnProductionOrCrash(TAG, "Incorrect value for switch");
        }

        builder.start(this);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        Log.d(TAG, "Destroying the manager.");
        if (billing != null) billing.destroyBillingInstance();
    }
}
